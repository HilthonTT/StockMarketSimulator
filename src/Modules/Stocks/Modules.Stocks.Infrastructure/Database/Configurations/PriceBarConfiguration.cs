using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Stocks.Domain.Entities;

namespace Modules.Stocks.Infrastructure.Database.Configurations;

internal sealed class PriceBarConfiguration : IEntityTypeConfiguration<PriceBar>
{
    public void Configure(EntityTypeBuilder<PriceBar> builder)
    {
        builder.ToTable("price_bars");

        // One bar per ticker and bucket; the feed upserts into the current minute.
        builder.HasKey(x => new { x.Ticker, x.BucketStart });

        builder.Property(x => x.Ticker).HasMaxLength(10);
    }
}
