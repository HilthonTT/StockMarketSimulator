using Modules.Stocks.Application.Abstractions.Realtime;
using Modules.Stocks.Contracts.Stocks;

namespace Modules.Stocks.Infrastructure.Market;

/// <summary>In-memory ring of the latest news. It's lost on restart; news is ephemeral flavour.</summary>
internal sealed class MarketNewsStore : IMarketNewsStore
{
    private const int Capacity = 100;

    private readonly Lock _lock = new();
    private readonly LinkedList<MarketNewsResponse> _items = new();

    public void Add(MarketNewsResponse news)
    {
        lock (_lock)
        {
            _items.AddFirst(news);

            while (_items.Count > Capacity)
            {
                _items.RemoveLast();
            }
        }
    }

    public IReadOnlyList<MarketNewsResponse> GetLatest(int count)
    {
        lock (_lock)
        {
            return [.. _items.Take(Math.Clamp(count, 1, Capacity))];
        }
    }
}
