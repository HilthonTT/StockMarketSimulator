using Microsoft.EntityFrameworkCore;
using Modules.Stocks.Application.Abstractions.Data;
using Stocks.Domain.Entities;

namespace Modules.Stocks.Infrastructure.Database;

internal sealed class StocksDbContext(DbContextOptions<StocksDbContext> options)
    : DbContext(options), IUnitOfWork, IDbContext
{
    public DbSet<Stock> Stocks { get; set; }

    public DbSet<StockSearchResult> StockSearchResults { get; set; }
}
