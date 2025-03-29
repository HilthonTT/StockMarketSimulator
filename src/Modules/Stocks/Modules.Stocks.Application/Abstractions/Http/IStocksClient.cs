using Modules.Stocks.Contracts.AlphaVantage;
using Modules.Stocks.Contracts.Stocks;

namespace Modules.Stocks.Application.Abstractions.Http;

public interface IStocksClient
{
    Task<StockPriceResponse?> GetDataForTickerAsync(string ticker, CancellationToken cancellationToken = default);

    Task<AlphaVantageSearchData?> SearchTickerAsync(string searchTerm, CancellationToken cancellationToken = default);
}
