using StockMarketSimulator.Api.Modules.Stocks.Contracts;
using StockMarketSimulator.Api.Modules.Stocks.Infrastructure;

namespace StockMarketSimulator.Api.Modules.Stocks.Api;

internal sealed class StocksApi : IStocksApi
{
    private readonly IStockService _stockService;

    public StocksApi(IStockService stockService)
    {
        _stockService = stockService;
    }

    public async Task<StockPriceInfo?> GetLatestPriceAsync(string ticker, CancellationToken cancellationToken = default)
    {
        StockPriceResponse? stockPriceResponse = await _stockService.GetLatestStockPriceAsync(ticker, cancellationToken);

        if (stockPriceResponse is null)
        {
            return null;
        }

        return new StockPriceInfo(stockPriceResponse.Ticker, stockPriceResponse.Price);
    }
}
