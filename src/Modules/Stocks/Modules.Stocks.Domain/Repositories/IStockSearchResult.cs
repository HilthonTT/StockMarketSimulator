using Stocks.Domain.Entities;

namespace Stocks.Domain.Repositories;

public interface IStockSearchResult
{
    Task<List<StockSearchResult>> SearchAsync(string ticker, CancellationToken cancellationToken = default);

    void AddRange(IEnumerable<StockSearchResult> searchResults);
}
