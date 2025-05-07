using Infrastructure.Database.Specifications;
using Microsoft.EntityFrameworkCore;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Repositories;
using Modules.Budgeting.Infrastructure.Database;
using Modules.Budgeting.Infrastructure.Specifications;
using SharedKernel;

namespace Modules.Budgeting.Infrastructure.Repositories;

internal sealed class AuditLogRepository(BudgetingDbContext context) : IAuditLogRepository
{
    public async Task<Option<AuditLog>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        AuditLog? auditlog = await ApplySpecification(new AuditLogByIdSpecification(id))
            .FirstOrDefaultAsync(cancellationToken);

        return Option<AuditLog>.Some(auditlog);
    }

    public void Insert(AuditLog auditLog)
    {
        context.AuditLogs.Add(auditLog);
    }

    public void Remove(AuditLog auditLog)
    {
        context.AuditLogs.Remove(auditLog);
    }

    private IQueryable<AuditLog> ApplySpecification(Specification<AuditLog> specification)
    {
        return SpecificationEvaluator.GetQuery(context.AuditLogs, specification);
    }
}
