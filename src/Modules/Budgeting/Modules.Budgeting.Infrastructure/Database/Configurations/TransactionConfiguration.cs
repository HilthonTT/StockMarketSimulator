using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Budgeting.Domain.Entities;

namespace Modules.Budgeting.Infrastructure.Database.Configurations;

internal sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Ticker).HasMaxLength(10);

        builder.HasIndex(x => new { x.Ticker })
            .HasMethod("GIN")
            .IsTsVectorExpressionIndex("english");

        builder.HasIndex(x => x.UserId);

        builder.Ignore(x => x.TotalAmount);
    }
}
