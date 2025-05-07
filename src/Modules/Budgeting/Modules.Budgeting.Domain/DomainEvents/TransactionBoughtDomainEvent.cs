using SharedKernel;

namespace Modules.Budgeting.Domain.DomainEvents;

public sealed record TransactionBoughtDomainEvent(Guid Id, Guid TransactionId) : DomainEvent(Id);
