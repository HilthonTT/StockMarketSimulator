using Infrastructure.Caching;
using Infrastructure.Database.Configurations;
using Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

public sealed class GeneralDbContext(DbContextOptions<GeneralDbContext> options) : DbContext(options)
{
    public DbSet<OutboxMessageConsumer> OutboxMessageConsumers { get; set; }

    public DbSet<CacheItem> CacheItems { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());
        modelBuilder.ApplyConfiguration(new CacheConfiguration());

        modelBuilder.HasDefaultSchema(Schemas.General);
    }
}
