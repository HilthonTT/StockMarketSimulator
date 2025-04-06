using SharedKernel;

namespace Modules.Stocks.Api;

public interface IStocksApi
{
    Task<Option<StockApiResponse>> GetByTickerAsync(string ticker, CancellationToken cancellationToken = default);
}
