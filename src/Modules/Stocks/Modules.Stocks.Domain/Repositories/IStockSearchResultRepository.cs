using Modules.Stocks.Domain.Entities;

namespace Modules.Stocks.Domain.Repositories;

public interface IStockSearchResultRepository
{
    void AddRange(IEnumerable<StockSearchResult> searchResults);
}
