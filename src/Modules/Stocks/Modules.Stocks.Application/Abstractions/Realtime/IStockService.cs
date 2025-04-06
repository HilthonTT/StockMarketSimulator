using Modules.Stocks.Contracts.Stocks;
using SharedKernel;

namespace Modules.Stocks.Application.Abstractions.Realtime;

public interface IStockService
{
    Task<Option<StockPriceResponse>> GetLatestStockPriceAsync(string ticker, CancellationToken cancellationToken = default);
}
