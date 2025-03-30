using Infrastructure.Database.Configurations;
using Microsoft.EntityFrameworkCore;
using Modules.Stocks.Application.Abstractions.Data;
using Modules.Stocks.Domain.Entities;

namespace Modules.Stocks.Infrastructure.Database;

public sealed class StocksDbContext(DbContextOptions<StocksDbContext> options)
    : DbContext(options), IUnitOfWork, IDbContext
{
    public DbSet<Stock> Stocks { get; set; }

    public DbSet<StockSearchResult> StockSearchResults { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(StocksDbContext).Assembly);
        modelBuilder.ApplyConfiguration(new OutboxMessageConfiguration());

        modelBuilder.HasDefaultSchema(Schemas.Stocks);
    }
}
