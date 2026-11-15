using Modules.Stocks.Contracts.Stocks;

namespace Modules.Stocks.Application.Abstractions.Realtime;

/// <summary>Keeps the most recent simulated news in memory.</summary>
public interface IMarketNewsStore
{
    void Add(MarketNewsResponse news);

    /// <summary>Newest first.</summary>
    IReadOnlyList<MarketNewsResponse> GetLatest(int count);
}
