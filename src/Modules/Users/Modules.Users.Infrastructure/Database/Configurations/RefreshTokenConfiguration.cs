using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Users.Domain.Entities;

namespace Modules.Users.Infrastructure.Database.Configurations;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasOne(x => x.User)
            .WithMany()
            .HasForeignKey(x => x.UserId);

        // SHA-256 as 64 lower-case hex characters.
        builder.Property(x => x.TokenHash).HasMaxLength(64).IsRequired();

        builder.HasIndex(x => x.TokenHash).IsUnique();

        // Used by the cleanup job (expired tokens, per-user cap).
        builder.HasIndex(x => new { x.UserId, x.ExpiresOnUtc });
    }
}
