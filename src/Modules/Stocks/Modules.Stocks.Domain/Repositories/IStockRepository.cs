using Modules.Stocks.Domain.Entities;

namespace Modules.Stocks.Domain.Repositories;

public interface IStockRepository
{
    Task<Stock?> GetByTickerAsync(string ticker, CancellationToken cancellationToken = default);

    Task<Stock?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Insert(Stock stock);

    void AddRange(IEnumerable<Stock> stocks);

    void Remove(Stock stock);
}
