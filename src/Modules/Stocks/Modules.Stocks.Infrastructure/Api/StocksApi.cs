using Microsoft.EntityFrameworkCore;
using Modules.Stocks.Api;
using Modules.Stocks.Infrastructure.Database;

namespace Modules.Stocks.Infrastructure.Api;

internal sealed class StocksApi(StocksDbContext context) : IStocksApi
{
    public Task<StockApiResponse?> GetByTickerAsync(string ticker, CancellationToken cancellationToken = default)
    {
        return context.Stocks
            .Where(x => x.Ticker == ticker)
            .Select(x => new StockApiResponse(x.Ticker, x.Price))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
