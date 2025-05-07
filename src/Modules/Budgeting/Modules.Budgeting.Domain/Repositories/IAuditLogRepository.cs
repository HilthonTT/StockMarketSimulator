using Modules.Budgeting.Domain.Entities;
using SharedKernel;

namespace Modules.Budgeting.Domain.Repositories;

public interface IAuditLogRepository
{
    Task<Option<AuditLog>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Insert(AuditLog auditLog);

    void Remove(AuditLog auditLog);
}
