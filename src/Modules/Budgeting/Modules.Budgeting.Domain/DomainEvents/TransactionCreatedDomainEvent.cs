using SharedKernel;

namespace Modules.Budgeting.Domain.DomainEvents;

public sealed record TransactionCreatedDomainEvent(Guid TransactionId) : IDomainEvent;
