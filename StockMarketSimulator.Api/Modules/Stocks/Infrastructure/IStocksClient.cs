using StockMarketSimulator.Api.Modules.Stocks.Contracts;
using StockMarketSimulator.Api.Modules.Stocks.Domain;

namespace StockMarketSimulator.Api.Modules.Stocks.Infrastructure;

public interface IStocksClient
{
    Task<StockPriceResponse?> GetDataForTickerAsync(string ticker, CancellationToken cancellationToken = default);

    Task<List<Match>> SearchTickerAsync(string searchTerm, CancellationToken cancellationToken = default);
}
