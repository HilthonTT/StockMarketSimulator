using SharedKernel;

namespace Modules.Budgeting.Domain.DomainEvents;

public sealed record TransactionBoughtDomainEvent(Guid TransactionId) : IDomainEvent;
