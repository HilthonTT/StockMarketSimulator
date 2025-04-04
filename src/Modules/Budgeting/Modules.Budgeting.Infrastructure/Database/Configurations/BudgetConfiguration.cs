using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Budgeting.Domain.Entities;

namespace Modules.Budgeting.Infrastructure.Database.Configurations;

internal sealed class BudgetConfiguration : IEntityTypeConfiguration<Budget>
{
    public void Configure(EntityTypeBuilder<Budget> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.UserId).IsUnique();

        builder.ToTable(tb =>
            tb.HasCheckConstraint("CK_buying_power_NotNegative", "buying_power > 0"));
    }
}
