using Microsoft.EntityFrameworkCore;
using Modules.Stocks.Api;
using Modules.Stocks.Infrastructure.Database;
using SharedKernel;

namespace Modules.Stocks.Infrastructure.Api;

internal sealed class StocksApi(StocksDbContext context) : IStocksApi
{
    public async Task<Option<StockApiResponse>> GetByTickerAsync(string ticker, CancellationToken cancellationToken = default)
    {
        StockApiResponse? stockApi = await context.Stocks
            .AsNoTracking()
            .Where(x => x.Ticker == ticker)
            .Select(x => new StockApiResponse(x.Ticker, x.Price))
            .FirstOrDefaultAsync(cancellationToken);

        return Option<StockApiResponse>.Some(stockApi);
    }
}
