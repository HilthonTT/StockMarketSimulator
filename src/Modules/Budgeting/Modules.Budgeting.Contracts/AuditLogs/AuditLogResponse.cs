using Modules.Budgeting.Domain.Enums;

namespace Modules.Budgeting.Contracts.AuditLogs;

public sealed record AuditLogResponse(
    Guid Id,
    Guid UserId,
    AuditLogType LogType,
    string Action,
    string? Description,
    Guid? RelatedEntityId,
    string? RelatedEntityType,
    DateTime CreatedOnUtc,
    DateTime? ModifiedOnUtc);
