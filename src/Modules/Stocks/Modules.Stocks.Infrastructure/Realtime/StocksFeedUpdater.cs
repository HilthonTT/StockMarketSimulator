using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.Stocks.Application.Abstractions.Realtime;
using Modules.Stocks.Contracts.Stocks;
using Modules.Stocks.Domain.Services;
using Modules.Stocks.Infrastructure.Realtime.Options;
using Quartz;
using SharedKernel;

namespace Modules.Stocks.Infrastructure.Realtime;

/// <summary>
/// Advances the simulated price of every active ticker by one GBM step and pushes it to SignalR clients.
/// </summary>
/// <remarks>
/// Each new price is persisted, so the next tick continues from it (a real random walk) and the
/// buy/sell handlers trade at exactly the price users see on the feed.
/// </remarks>
[DisallowConcurrentExecution]
public sealed class StocksFeedUpdater(
    IActiveTickerManager activeTickerManager,
    IServiceScopeFactory serviceScopeFactory,
    IHubContext<StocksFeedHub, IStocksUpdateClient> hubContext,
    IOptions<StockUpdateOptions> options,
    ILogger<StocksFeedUpdater> logger,
    IStockVolatilityProvider stockVolatilityProvider) : IJob
{
    public const string Name = nameof(StocksFeedUpdater);

    private readonly StockUpdateOptions _options = options.Value;

    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        IReadOnlyCollection<string> tickers = activeTickerManager.GetAllTickers();
        if (tickers.Count == 0)
        {
            return;
        }

        using IServiceScope scope = serviceScopeFactory.CreateScope();
        IStockService stockService = scope.ServiceProvider.GetRequiredService<IStockService>();

        var parallelOptions = new ParallelOptions
        {
            MaxDegreeOfParallelism = 5,
            CancellationToken = cancellationToken,
        };

        await Parallel.ForEachAsync(tickers, parallelOptions, async (ticker, token) =>
        {
            // One failing ticker must not cancel the update of all the others.
            try
            {
                await UpdateTickerAsync(stockService, ticker, token);
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogError(exception, "Failed to update simulated price for {Ticker}", ticker);
            }
        });
    }

    private async Task UpdateTickerAsync(IStockService stockService, string ticker, CancellationToken cancellationToken)
    {
        Option<StockPriceResponse> optionCurrentPrice =
            await stockService.GetLatestStockPriceAsync(ticker, cancellationToken);

        if (!optionCurrentPrice.IsSome)
        {
            return;
        }

        StockPriceResponse currentPrice = optionCurrentPrice.ValueOrThrow();

        (double mu, double sigma) = stockVolatilityProvider.GetParameters(currentPrice.Ticker);

        decimal newPrice = GeometricBrownianMotion.NextPrice(
            currentPrice.Price,
            dailyDrift: mu,
            dailyVolatility: sigma,
            stepInTradingDays: _options.UpdateIntervalInSeconds / GeometricBrownianMotion.TradingSecondsPerDay,
            maxStepChange: _options.MaxPercentageChange,
            standardNormal: GeometricBrownianMotion.SampleStandardNormal(Random.Shared));

        await stockService.UpdatePriceAsync(currentPrice.Ticker, newPrice, cancellationToken);

        await hubContext.Clients
            .Group(currentPrice.Ticker)
            .ReceiveStockPriceUpdate(new StockPriceUpdate(currentPrice.Ticker, newPrice), cancellationToken);

        logger.LogDebug("Updated {Ticker} price to {Price}", currentPrice.Ticker, newPrice);
    }
}
