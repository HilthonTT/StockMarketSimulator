using Modules.Budgeting.Domain.Enums;
using SharedKernel;

namespace Modules.Budgeting.Domain.DomainEvents;

public sealed record TransactionCreatedDomainEvent(Guid TransactionId, TransactionType Type) : IDomainEvent;
