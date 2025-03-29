using System.Collections.Concurrent;
using Modules.Stocks.Application.Abstractions.Realtime;

namespace Modules.Stocks.Infrastructure.Realtime;

internal sealed class ActiveTickerManager : IActiveTickerManager
{
    private readonly ConcurrentBag<string> _activeTickers = [];
    
    public void AddTicker(string ticker)
    {
        if (!_activeTickers.Contains(ticker))
        {
            _activeTickers.Add(ticker);
        }
    }

    public IReadOnlyCollection<string> GetAllTickers() => [.._activeTickers];
}
