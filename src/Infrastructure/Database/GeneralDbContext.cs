using Infrastructure.Database.Configurations;
using Infrastructure.Outbox;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Database;

public sealed class GeneralDbContext(DbContextOptions<GeneralDbContext> options) : DbContext(options)
{
    public DbSet<OutboxMessageConsumer> OutboxMessageConsumers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new OutboxMessageConsumerConfiguration());

        modelBuilder.HasDefaultSchema(Schemas.General);
    }
}
