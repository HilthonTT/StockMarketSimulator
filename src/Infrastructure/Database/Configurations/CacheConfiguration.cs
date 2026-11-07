using Infrastructure.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

internal sealed class CacheConfiguration : IEntityTypeConfiguration<CacheItem>
{
    public const string TableName = "cache_items";

    public void Configure(EntityTypeBuilder<CacheItem> builder)
    {
        builder.ToTable(TableName);

        builder.HasKey(x => x.Key);

        builder.Property(x => x.Value).HasColumnType("jsonb");

        builder.HasIndex(x => x.ExpiresOnUtc);
    }
}
