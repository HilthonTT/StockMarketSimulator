using Infrastructure.Database.Specifications;
using Modules.Budgeting.Domain.Entities;

namespace Modules.Budgeting.Infrastructure.Specifications;

internal sealed class AuditLogByIdSpecification : Specification<AuditLog>
{
    public AuditLogByIdSpecification(Guid auditlogId) 
        : base(auditlog => auditlog.Id == auditlogId)
    {
    }
}
