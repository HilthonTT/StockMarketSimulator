using Modules.Stocks.Api.Responses;
using SharedKernel;

namespace Modules.Stocks.Api.Api;

public interface IStocksApi
{
    Task<Option<StockApiResponse>> GetByTickerAsync(string ticker, CancellationToken cancellationToken = default);
}
