using Modules.Stocks.Contracts.Stocks;
using SharedKernel;

namespace Modules.Stocks.Application.Abstractions.Realtime;

public interface IStockService
{
    Task<Option<StockPriceResponse>> GetLatestStockPriceAsync(string ticker, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists a new (simulated) price so the feed, quotes and trades all use the same price.
    /// </summary>
    Task UpdatePriceAsync(string ticker, decimal price, CancellationToken cancellationToken = default);
}
