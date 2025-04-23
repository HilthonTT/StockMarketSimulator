using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Repositories;
using Modules.Budgeting.Infrastructure.Database;

namespace Modules.Budgeting.Infrastructure.Repositories;

internal sealed class AuditLogRepository(BudgetingDbContext context) : IAuditLogRepository
{
    public void Insert(AuditLog auditLog)
    {
        context.AuditLogs.Add(auditLog);
    }

    public void Remove(AuditLog auditLog)
    {
        context.AuditLogs.Remove(auditLog);
    }
}
