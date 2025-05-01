using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Modules.Budgeting.Domain.Entities;

namespace Modules.Budgeting.Infrastructure.Database.Configurations;

internal sealed class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(x => x.Id);

        builder.HasIndex(x => x.UserId);

        builder.HasIndex(x => new { x.Action, x.Description })
           .HasMethod("GIN")
           .IsTsVectorExpressionIndex("english");

        builder.HasQueryFilter(x => !x.IsDeleted);

        builder.HasIndex(x => x.IsDeleted).HasFilter("is_deleted = true");
    }
}
