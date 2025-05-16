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

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("UpdateLatestStockPriceJob started at {Time}", dateTimeProvider.UtcNow);

        using IServiceScope scope = serviceScopeFactory.CreateScope();

        IStocksClient stocksClient = scope.ServiceProvider.GetRequiredService<IStocksClient>();
        IDbContext dbContext = scope.ServiceProvider.GetRequiredService<IDbContext>();
        IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

        DateTime thresholdDate = dateTimeProvider.UtcNow.AddDays(-DaysInPast);

        using IDbTransaction transaction = await dbContext.BeginTransactionAsync(context.CancellationToken);

        Dictionary<string, decimal> tickerToPrice = new(StringComparer.OrdinalIgnoreCase);

        try
        {
            List<Stock> outdatedStocks = await dbContext.Stocks
                .Where(s =>
                    (s.ModifiedOnUtc.HasValue && s.ModifiedOnUtc < thresholdDate) ||
                    (!s.ModifiedOnUtc.HasValue && s.CreatedOnUtc < thresholdDate))
                .Take(BatchSize)
                .ToListAsync(context.CancellationToken);

            logger.LogInformation("Found {Count} outdated stock(s) to update.", outdatedStocks.Count);

            foreach (Stock outdatedStock in outdatedStocks)
            {
                if (!tickerToPrice.TryGetValue(outdatedStock.Ticker, out decimal price))
                {
                    StockPriceResponse? priceResponse = 
                        await stocksClient.GetDataForTickerAsync(outdatedStock.Ticker, context.CancellationToken);

                    if (priceResponse is null)
                    {
                        logger.LogWarning("No price data returned for ticker '{Ticker}'. Skipping.", outdatedStock.Ticker);
                        continue; // skip if no price found
                    }

                    price = priceResponse.Price;
                    tickerToPrice[outdatedStock.Ticker] = price;
                }

                outdatedStock.ChangePrice(price);
            }

            await unitOfWork.SaveChangesAsync(context.CancellationToken);

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
