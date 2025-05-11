using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.Stocks.Application.Abstractions.Realtime;
using Modules.Stocks.Contracts.Stocks;
using Quartz;
using SharedKernel;

namespace Modules.Stocks.Infrastructure.Realtime;

[DisallowConcurrentExecution]
public sealed class StocksFeedUpdater(
    IActiveTickerManager activeTickerManager,
    IServiceScopeFactory serviceScopeFactory,
    IHubContext<StocksFeedHub, IStocksUpdateClient> hubContext,
    ILogger<StocksFeedUpdater> logger,
    IStockVolatilityProvider stockVolatilityProvider) : IJob
{
    public const string Name = nameof(StocksFeedUpdater);

    public Task Execute(IJobExecutionContext context)
    {
        return UpdateStockPricesAsync(context.CancellationToken);
    }

    private async Task UpdateStockPricesAsync(CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        IStockService stockService = scope.ServiceProvider.GetRequiredService<IStockService>();

        IReadOnlyCollection<string> tickers = activeTickerManager.GetAllTickers();

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = 5,
            CancellationToken = cancellationToken,
        };

        await Parallel.ForEachAsync(tickers, parallelOptions, async (ticker, token) =>
        {
            Option<StockPriceResponse> optionCurrentPrice =
                    await stockService.GetLatestStockPriceAsync(ticker, token);

            if (!optionCurrentPrice.IsSome)
            {
                return;
            }

            StockPriceResponse currentPrice = optionCurrentPrice.ValueOrThrow();

            decimal newPrice = CalculateNewPrice(currentPrice);

            var update = new StockPriceUpdate(ticker, newPrice);

            await hubContext.Clients.Group(ticker).ReceiveStockPriceUpdate(update, token);

            logger.LogInformation("Updated {Ticker} price to {Price}", ticker, newPrice);
        });
    }

    private decimal CalculateNewPrice(StockPriceResponse currentPrice)
    {
        const double dt = 1.0 / 252; // Daily step assuming 252 trading days/year
        (double mu, double sigma) = stockVolatilityProvider.GetParameters(currentPrice.Ticker);

        double standardNormal = BoxMuller();
        double percentageChange = (mu * dt) + (sigma * Math.Sqrt(dt) * standardNormal);

        // Occasionally inject a rare but significant price shock
        if (Random.Shared.NextDouble() < 0.01) // ~1% chance per update
        {
            double shockMagnitude = (Random.Shared.NextDouble() - 0.5) * 0.3; // +- 30% max
            percentageChange += shockMagnitude;
        }

        // Limit the maximum/minimum value of percentageChange to avoid excessive values
        percentageChange = Math.Clamp(percentageChange, -0.90, 0.90);  // Limit price changes to -90% to +90%

        // Ensure no overflow happens with the price calculation
        decimal priceFactor = (decimal)(1 + percentageChange);
        if (priceFactor < 0)
        {
            priceFactor = 0; // Ensure price never goes negative
        }

        decimal newPrice = currentPrice.Price * priceFactor;
        newPrice = Math.Max(newPrice, 0); // Prevent negative prices

        // Ensure the new price is within a reasonable range to avoid OverflowException
        newPrice = Math.Round(newPrice, 2);

        return newPrice;
    }

    /// <summary>
    /// Generates a standard normally distributed value using the Box-Muller transform.
    /// </summary>
    /// <param name="rand">Random number generator.</param>
    /// <returns>Standard normal variable (mean 0, std dev 1).</returns>
    private static double BoxMuller()
    {
        double u1 = 1.0 - Random.Shared.NextDouble(); // (0,1]
        double u2 = 1.0 - Random.Shared.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
    }
}
