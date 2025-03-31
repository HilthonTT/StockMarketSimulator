namespace Modules.Stocks.Api;

public interface IStocksApi
{
    Task<StockApiResponse?> GetByTickerAsync(string ticker, CancellationToken cancellationToken = default);
}
