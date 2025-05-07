using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Enums;
using Modules.Budgeting.Domain.ValueObjects;

namespace Modules.Budgeting.Infrastructure.Database.Configurations;

internal sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Ticker).HasMaxLength(10);

        builder.Property(x => x.Type)
            .HasConversion(p => p.Id, v => TransactionType.FromId(v)!)
            .IsRequired();

        builder.OwnsOne(x => x.Money, moneyBuilder =>
        {
            moneyBuilder.Property(money => money.Currency)
                .HasConversion(currency => currency.Code, code => Currency.FromCode(code));
        });

        builder.HasIndex(x => new { x.Ticker })
            .HasMethod("GIN")
            .IsTsVectorExpressionIndex("english");

        builder.HasIndex(x => x.UserId);

        builder.Ignore(x => x.TotalAmount);
    }
}
