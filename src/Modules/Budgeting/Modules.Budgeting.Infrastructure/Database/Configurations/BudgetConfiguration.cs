using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.ValueObjects;

namespace Modules.Budgeting.Infrastructure.Database.Configurations;

internal sealed class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.HasKey(x => x.Id);

        builder.OwnsOne(x => x.Money, moneyBuilder =>
        {
            moneyBuilder.Property(money => money.Currency)
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code));
        });

        builder.HasIndex(x => x.UserId).IsUnique();

        builder.ToTable(tb =>
            tb.HasCheckConstraint("CK_buying_power_NotNegative", "buying_power > 0"));
    }
}
