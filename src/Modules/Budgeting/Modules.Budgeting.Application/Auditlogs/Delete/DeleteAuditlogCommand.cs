using Application.Abstractions.Messaging;

namespace Modules.Budgeting.Application.Auditlogs.Delete;

public sealed record DeleteAuditlogCommand(Guid AuditlogId) : ICommand;
