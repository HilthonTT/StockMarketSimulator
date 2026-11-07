using System.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.Stocks.Application.Abstractions.Data;
using Modules.Stocks.Application.Abstractions.Http;
using Modules.Stocks.Contracts.Stocks;
using Modules.Stocks.Domain.Entities;
using Quartz;
using SharedKernel;

namespace Modules.Stocks.BackgroundJobs.Stocks;

[DisallowConcurrentExecution]
public sealed class UpdateLatestStockPriceJob(
    ILogger<UpdateLatestStockPriceJob> logger,
    IServiceScopeFactory serviceScopeFactory,
    IDateTimeProvider dateTimeProvider) : IJob
{
    private const int DaysInPast = 3;
    private const int BatchSize = 100;

    public const string Name = nameof(UpdateLatestStockPriceJob);

    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("UpdateLatestStockPriceJob started at {Time}", dateTimeProvider.UtcNow);

        using IServiceScope scope = serviceScopeFactory.CreateScope();

        IStocksClient stocksClient = scope.ServiceProvider.GetRequiredService<IStocksClient>();
        IDbContext dbContext = scope.ServiceProvider.GetRequiredService<IDbContext>();
        IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        DateTime thresholdDate = dateTimeProvider.UtcNow.AddDays(-DaysInPast);

        using IDbTransaction transaction = await dbContext.BeginTransactionAsync(cancellationToken);

        Dictionary<string, decimal> tickerToPrice = new(StringComparer.OrdinalIgnoreCase);

        try
        {
            List<Stock> outdatedStocks = await dbContext.Stocks
                .Where(s =>
                    (s.ModifiedOnUtc.HasValue && s.ModifiedOnUtc < thresholdDate) ||
                    (!s.ModifiedOnUtc.HasValue && s.CreatedOnUtc < thresholdDate))
                .OrderBy(s => s.ModifiedOnUtc ?? s.CreatedOnUtc) // oldest first, so every stock gets its turn
                .Take(BatchSize)
                .ToListAsync(cancellationToken);

            logger.LogInformation("Found {Count} outdated stock(s) to update.", outdatedStocks.Count);

            foreach (Stock outdatedStock in outdatedStocks)
            {
                if (!tickerToPrice.TryGetValue(outdatedStock.Ticker, out decimal price))
                {
                    StockPriceResponse? priceResponse;

                    try
                    {
                        priceResponse = await stocksClient.GetDataForTickerAsync(outdatedStock.Ticker, cancellationToken);
                    }
                    catch (Exception exception) when (exception is not OperationCanceledException)
                    {
                        // One ticker failing (rate limit, delisting, ...) must not roll back the whole batch.
                        logger.LogWarning(exception, "Fetching price for ticker '{Ticker}' failed. Skipping.", outdatedStock.Ticker);
                        continue;
                    }

                    if (priceResponse is null || priceResponse.Price <= 0)
                    {
                        logger.LogWarning("No valid price data returned for ticker '{Ticker}'. Skipping.", outdatedStock.Ticker);
                        continue;
                    }

                    price = priceResponse.Price;
                    tickerToPrice[outdatedStock.Ticker] = price;
                }

                outdatedStock.ChangePrice(price);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            transaction.Commit();

            logger.LogInformation("UpdateLatestStockPriceJob completed successfully at {Time}", dateTimeProvider.UtcNow);
        }
        catch (Exception ex)
        {
            transaction.Rollback();
            logger.LogError(ex, "An error occurred while updating stock prices.");
            throw;
        }
    }
}
