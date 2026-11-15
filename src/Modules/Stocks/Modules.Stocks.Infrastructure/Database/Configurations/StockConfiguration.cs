using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Stocks.Domain.Entities;

namespace Modules.Stocks.Infrastructure.Database.Configurations;

internal sealed class StockConfiguration : IEntityTypeConfiguration<Stock>
{
    public void Configure(EntityTypeBuilder<Stock> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Ticker).HasMaxLength(10);
        builder.Property(x => x.Name).HasMaxLength(100);
        builder.Property(x => x.Sector).HasMaxLength(50);
        builder.Property(x => x.AssetClass).HasMaxLength(10).HasDefaultValue("stock");

        // One row per ticker: concurrent first lookups upsert instead of inserting duplicates.
        builder.HasIndex(x => x.Ticker).IsUnique();
        builder.HasIndex(x => x.CreatedOnUtc);
    }
}
