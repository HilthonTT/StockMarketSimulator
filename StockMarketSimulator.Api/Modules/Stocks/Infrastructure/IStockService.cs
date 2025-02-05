using StockMarketSimulator.Api.Modules.Stocks.Contracts;

namespace StockMarketSimulator.Api.Modules.Stocks.Infrastructure;

internal interface IStockService
{
    Task<StockPriceResponse?> GetLatestStockPriceAsync(string ticker, CancellationToken cancellationToken = default);
}
