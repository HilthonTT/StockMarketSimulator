namespace StockMarketSimulator.Api.Modules.Stocks.Api;

public interface IStocksApi
{
    Task<StockPriceInfo?> GetLatestPriceAsync(string ticker, CancellationToken cancellationToken = default);
}
