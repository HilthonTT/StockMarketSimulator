using Modules.Stocks.Contracts.Stocks;
using SharedKernel;

namespace Modules.Stocks.Application.Abstractions.Realtime;

public interface IStockService
{
    /// <summary>
    /// Gets the stored price (fetching and storing it on first use) and marks the ticker as recently
    /// requested, so the feed simulates it for a while.
    /// </summary>
    Task<Option<StockPriceResponse>> GetLatestStockPriceAsync(string ticker, CancellationToken cancellationToken = default);

    /// <summary>
    /// Same as <see cref="GetLatestStockPriceAsync(string, CancellationToken)"/>, with control over whether
    /// the read refreshes the ticker's activity. The feed passes <c>false</c>; otherwise its own reads
    /// would keep every ticker active forever.
    /// </summary>
    Task<Option<StockPriceResponse>> GetLatestStockPriceAsync(
        string ticker,
        bool trackActivity,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new (simulated) price so the feed, quotes and trades all use the same price.
    /// </summary>
    Task UpdatePriceAsync(string ticker, decimal price, CancellationToken cancellationToken = default);
}
