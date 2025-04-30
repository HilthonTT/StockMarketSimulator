using Application.Abstractions.Messaging;
using Contracts.Common;
using Modules.Budgeting.Contracts.AuditLogs;

namespace Modules.Budgeting.Application.Auditlogs.GetByUserId;

public sealed record GetAuditlogsByUserIdQuery(
    Guid UserId,
    string? SearchTerm,
    int Page,
    int PageSize,
    int? LogType) : IQuery<PagedList<AuditLogResponse>>;
