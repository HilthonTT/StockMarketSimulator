using Modules.Stocks.Domain.Entities;
using Modules.Stocks.Domain.Repositories;
using Modules.Stocks.Infrastructure.Database;

namespace Modules.Stocks.Infrastructure.Repositories;

internal sealed class StockSearchResultRepository(StocksDbContext context) : IStockSearchResultRepository
{
    public void AddRange(IEnumerable<StockSearchResult> searchResults)
    {
        context.StockSearchResults.AddRange(searchResults);
    }
}
