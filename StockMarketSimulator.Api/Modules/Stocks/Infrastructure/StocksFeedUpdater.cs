using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Options;
using StockMarketSimulator.Api.Infrastructure.Database;

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
        using var scope = _serviceScopeFactory.CreateScope();
    }
}
