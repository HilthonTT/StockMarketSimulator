using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using StockMarketSimulator.Api.Infrastructure.Database;
using StockMarketSimulator.Api.Modules.Stocks.Contracts;

namespace StockMarketSimulator.Api.Modules.Stocks.Infrastructure;

internal sealed class StocksFeedUpdater : BackgroundService
{
    private readonly Random _random = new();
    private readonly ActiveTickerManager _activeTickerManager;
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly IHubContext<StocksFeedHub, IStocksUpdateClient> _hubContext;
    private readonly StockUpdateOptions _options;
    private readonly ILogger<StocksFeedUpdater> _logger;
    private readonly DatabaseInitializationCompletionSignal _signal;

    public StocksFeedUpdater(
        ActiveTickerManager activeTickerManager,
        IServiceScopeFactory serviceScopeFactory,
        IHubContext<StocksFeedHub, IStocksUpdateClient> hubContext,
        IOptions<StockUpdateOptions> options,
        ILogger<StocksFeedUpdater> logger,
        DatabaseInitializationCompletionSignal signal)
    {
        _activeTickerManager = activeTickerManager;
        _serviceScopeFactory = serviceScopeFactory;
        _hubContext = hubContext;
        _options = options.Value;
        _logger = logger;
        _signal = signal;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _signal.WaitForInitializationAsync();

        while (!stoppingToken.IsCancellationRequested)
        {
            await UpdateStockPricesAsync(stoppingToken);

            await Task.Delay(_options.UpdateInterval, stoppingToken);
        }
    }

    private async Task UpdateStockPricesAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();
        IStockService stockService = scope.ServiceProvider.GetRequiredService<IStockService>();

        foreach (string ticker in _activeTickerManager.GetAllTickers())
        {
            StockPriceResponse? currentPrice = await stockService.GetLatestStockPriceAsync(ticker, cancellationToken);
            if (currentPrice is null)
            {
                continue;
            }

            decimal newPrice = CalculateNewPrice(currentPrice);

            var update = new StockPriceUpdate(ticker, newPrice);

            await _hubContext.Clients.Group(ticker).ReceiveStockPriceUpdate(update, cancellationToken);

            _logger.LogInformation("Updated {Ticker} price to {Price}", ticker, newPrice);
        }
    }

    private decimal CalculateNewPrice(StockPriceResponse currentPrice)
    {
        double change = _options.MaxPercentageChange;

        decimal priceFactor = (decimal)(_random.NextDouble() * change * 2 - change);
        decimal priceChange = currentPrice.Price * priceFactor;
        decimal newPrice = Math.Max(0, currentPrice.Price + priceChange);

        newPrice = Math.Round(newPrice, 2);

        return newPrice;
    }
}
