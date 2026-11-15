using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Budgeting.Domain.Entities;

namespace Modules.Budgeting.Infrastructure.Database.Configurations;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Ticker).HasMaxLength(10).IsRequired();

        builder.Property(x => x.RejectionReason).HasMaxLength(300);

        builder.Ignore(x => x.IsPending);

        // The fill job scans pending orders; users list their own.
        builder.HasIndex(x => x.Status).HasFilter("status = 1");
        builder.HasIndex(x => new { x.UserId, x.CreatedOnUtc });

        // Two fill attempts racing on one order can't both win.
        builder.Property<uint>("Version").IsRowVersion();

        builder.ToTable(tb =>
        {
            tb.HasCheckConstraint("CK_orders_quantity_Positive", "quantity > 0");
            tb.HasCheckConstraint("CK_orders_trigger_price_Positive", "trigger_price > 0");
        });
    }
}
