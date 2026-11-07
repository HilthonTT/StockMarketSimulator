using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Budgeting.Domain.Entities;

namespace Modules.Budgeting.Infrastructure.Database.Configurations;

internal sealed class PositionConfiguration : IEntityTypeConfiguration<Position>
{
    public void Configure(EntityTypeBuilder<Position> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Ticker).HasMaxLength(10).IsRequired();

        builder.Property(x => x.TotalCost).HasColumnType("numeric");

        // One row per holding; also what makes two concurrent "first buys" of a ticker collide.
        builder.HasIndex(x => new { x.UserId, x.Ticker }).IsUnique();

        builder.Ignore(x => x.AverageCost);

        builder.ToTable(tb =>
        {
            tb.HasCheckConstraint("CK_positions_quantity_NotNegative", "quantity >= 0");
            tb.HasCheckConstraint("CK_positions_total_cost_NotNegative", "total_cost >= 0");
        });
    }
}
