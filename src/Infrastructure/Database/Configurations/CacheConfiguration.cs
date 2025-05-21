using Infrastructure.Caching;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.Database.Configurations;

internal sealed class CacheConfiguration : IEntityTypeConfiguration<CacheItem>
{
    public void Configure(EntityTypeBuilder<CacheItem> builder)
    {
        builder.HasKey(x => x.Key);

        builder.Property(x => x.Value).HasColumnType("jsonb");
    }
}
