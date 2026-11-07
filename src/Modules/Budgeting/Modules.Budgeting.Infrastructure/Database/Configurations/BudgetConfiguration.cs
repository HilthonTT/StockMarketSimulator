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

        // Optimistic concurrency via PostgreSQL's xmin system column: every buy/sell rewrites the
        // budget row, so two concurrent trades for the same user cannot both commit (no double spend,
        // no overselling) - the loser gets a DbUpdateConcurrencyException.
        builder.Property<uint>("Version").IsRowVersion();

        // A balance of exactly zero is valid (the user spent everything).
        builder.ToTable(tb =>
            tb.HasCheckConstraint("CK_budgets_money_amount_NotNegative", "money_amount >= 0"));
    }
}
