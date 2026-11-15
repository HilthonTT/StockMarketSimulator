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
        // Concurrency token: rotation updates "WHERE id = @id AND token_hash = @old", so two concurrent
        // refreshes with the same token can't both succeed (the loser gets DbUpdateConcurrencyException).
        builder.Property(x => x.TokenHash).HasMaxLength(64).IsRequired().IsConcurrencyToken();

        builder.HasIndex(x => x.TokenHash).IsUnique();

        // Used by the cleanup job (expired tokens, per-user cap).
        builder.HasIndex(x => new { x.UserId, x.ExpiresOnUtc });
    }
}
