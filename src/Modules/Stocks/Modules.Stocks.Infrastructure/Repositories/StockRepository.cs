using Microsoft.EntityFrameworkCore;
using Modules.Stocks.Domain.Entities;
using Modules.Stocks.Domain.Repositories;
using Modules.Stocks.Infrastructure.Database;

namespace Modules.Stocks.Infrastructure.Repositories;

internal sealed class StockRepository(StocksDbContext context) : IStockRepository
{
    public void AddRange(IEnumerable<Stock> stocks)
    {
        context.Stocks.AddRange(stocks);
    }

    public Task<Stock?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Stocks.FirstOrDefaultAsync(s => s.Id == id, cancellationToken);
    }

    public Task<Stock?> GetByTickerAsync(string ticker, CancellationToken cancellationToken = default)
    {
        return context.Stocks.FirstOrDefaultAsync(s => s.Ticker == ticker, cancellationToken);
    }

    public void Insert(Stock stock)
    {
        context.Stocks.Add(stock);
    }

    public void Remove(Stock stock)
    {
        context.Stocks.Remove(stock);
    }
}
