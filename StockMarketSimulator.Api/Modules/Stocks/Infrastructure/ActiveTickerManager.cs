using System.Collections.Concurrent;

namespace StockMarketSimulator.Api.Modules.Stocks.Infrastructure;

internal sealed class ActiveTickerManager
{
    private readonly ConcurrentBag<string> _activeTickers = [];

    public void AddTicker(string ticker)
    {
        if (_activeTickers.Contains(ticker))
        {
            _activeTickers.Add(ticker);
        }
    }

    public IReadOnlyCollection<string> GetAllTickers() => [.. _activeTickers];
}
