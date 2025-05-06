using Microsoft.EntityFrameworkCore;
using Modules.Stocks.Api.Api;
using Modules.Stocks.Api.Responses;
using Modules.Stocks.Infrastructure.Database;
using SharedKernel;

namespace Modules.Stocks.Infrastructure.Api;

internal sealed class StocksApi(StocksDbContext context) : IStocksApi
{
    private static readonly Func<StocksDbContext, string, Task<StockApiResponse?>> GetStockByTicker =
        EF.CompileAsyncQuery((StocksDbContext dbContext, string id) =>
            dbContext.Stocks
                .Where(s => s.Ticker == id)
                .Select(s => new StockApiResponse(s.Ticker, s.Price))
                .FirstOrDefault());

    public async Task<Option<StockApiResponse>> GetByTickerAsync(
        string ticker, 
        CancellationToken cancellationToken = default)
    {
        StockApiResponse? stockApi = await GetStockByTicker(context, ticker);

        return Option<StockApiResponse>.Some(stockApi);
    }
}
