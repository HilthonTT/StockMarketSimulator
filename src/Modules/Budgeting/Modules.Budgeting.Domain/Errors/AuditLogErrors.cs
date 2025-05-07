using SharedKernel;

namespace Modules.Budgeting.Domain.Errors;

public static class AuditLogErrors
{
    public static Error NotFound(Guid id) => Error.NotFound(
        "AuditLog.NotFound",
        $"The auditlog with the Id = '{id}' was not found");
}
