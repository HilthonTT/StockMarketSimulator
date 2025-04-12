using SharedKernel;

namespace Modules.Budgeting.Domain.DomainEvents;

public sealed record TransactionSoldDomainEvent(Guid TransactionId) : IDomainEvent;
