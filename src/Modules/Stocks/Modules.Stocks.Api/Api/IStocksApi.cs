using Modules.Stocks.Api.Responses;
using SharedKernel;

namespace Modules.Stocks.Api.Api;

public interface IStocksApi
{
    Task<Option<StockApiResponse>> GetByTickerAsync(string ticker, CancellationToken cancellationToken = default);

    /// <summary>Current prices keyed by upper-case ticker; unknown tickers are left out.</summary>
    Task<IReadOnlyDictionary<string, decimal>> GetPricesAsync(
        IReadOnlyCollection<string> tickers,
        CancellationToken cancellationToken = default);
}
