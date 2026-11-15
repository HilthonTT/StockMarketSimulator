using Modules.Stocks.Contracts.Stocks;

namespace Modules.Stocks.Application.Abstractions.Realtime;

public interface IStocksUpdateClient
{
    Task ReceiveStockPriceUpdate(StockPriceUpdate update, CancellationToken cancellationToken = default);

    Task ReceiveMarketNews(MarketNewsResponse news, CancellationToken cancellationToken = default);
}
