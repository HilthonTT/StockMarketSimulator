using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Budgeting.Domain.Entities;

namespace Modules.Budgeting.Infrastructure.Database.Configurations;

internal sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.UserId);

        builder.Ignore(x => x.TotalAmount);
    }
}
