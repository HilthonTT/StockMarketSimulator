using System.Collections.Concurrent;
using Modules.Stocks.Application.Abstractions.Realtime;

namespace Modules.Stocks.Infrastructure.Realtime;

internal sealed class ActiveTickerManager : IActiveTickerManager
{
    private readonly ConcurrentDictionary<string, bool> _activeTickers = [];

    /// <summary>
    /// Adds a ticker to the active tickers list if it isn't already present.
    /// </summary>
    /// <param name="ticker">The ticker symbol to add.</param>
    public bool AddTicker(string ticker)
    {
        return _activeTickers.TryAdd(ticker, true);
    }

    /// <summary>
    /// Gets all active tickers.
    /// </summary>
    /// <returns>A read-only collection of active tickers</returns>
    public IReadOnlyCollection<string> GetAllTickers() => 
        _activeTickers.Keys.ToList().AsReadOnly();
}
