using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Stocks.Domain.Entities;

namespace Modules.Stocks.Infrastructure.Database.Configurations;

internal sealed class ShortenUrlConfiguration : IEntityTypeConfiguration<ShortenedUrl>
{
    public void Configure(EntityTypeBuilder<ShortenedUrl> builder)
    {
        builder.ToTable("shortened_urls");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ShortCode).HasMaxLength(10);

        builder.HasIndex(x => x.ShortCode).IsUnique();
    }
}
