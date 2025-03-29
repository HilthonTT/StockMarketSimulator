using Microsoft.EntityFrameworkCore;
using Stocks.Domain.Entities;

namespace Modules.Stocks.Application.Abstractions.Data;

public interface IDbContext
{
    DbSet<Stock> Stocks { get; }

    DbSet<StockSearchResult> StockSearchResults { get; }
}
