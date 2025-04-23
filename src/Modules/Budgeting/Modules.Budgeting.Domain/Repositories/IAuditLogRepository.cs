using Modules.Budgeting.Domain.Entities;

namespace Modules.Budgeting.Domain.Repositories;

public interface IAuditLogRepository
{
    void Insert(AuditLog auditLog);

    void Remove(AuditLog auditLog);
}
