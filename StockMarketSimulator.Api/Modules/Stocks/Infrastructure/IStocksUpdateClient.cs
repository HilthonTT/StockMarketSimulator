namespace StockMarketSimulator.Api.Modules.Stocks.Infrastructure;

public interface IStocksUpdateClient
{
    Task ReceiveStockPriceUpdate(StockPriceUpdate update, CancellationToken cancellationToken = default);
}
