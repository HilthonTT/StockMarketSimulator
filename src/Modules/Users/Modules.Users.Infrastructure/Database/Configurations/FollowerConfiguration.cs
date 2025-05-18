using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Users.Domain.Entities;

namespace Modules.Users.Infrastructure.Database.Configurations;

internal sealed class FollowerConfiguration : IEntityTypeConfiguration<Follower>
{
    public void Configure(EntityTypeBuilder<Follower> builder)
    {
        builder.HasKey(f => new { f.UserId, f.FollowedId });

        builder.HasIndex(f => new { f.UserId, f.FollowedId });

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(f => f.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(f => f.FollowedId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
