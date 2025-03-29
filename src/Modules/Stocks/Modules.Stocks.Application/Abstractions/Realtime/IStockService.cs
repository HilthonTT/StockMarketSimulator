using Modules.Stocks.Contracts.Stocks;

namespace Modules.Stocks.Application.Abstractions.Realtime;

public interface IStockService
{
    Task<StockPriceResponse?> GetLatestStockPriceAsync(string ticker, CancellationToken cancellationToken = default);
}
