using SharedKernel;

namespace Modules.Budgeting.Domain.DomainEvents;

public sealed record TransactionUpdatedDomainEvent(Guid TransactionId) : IDomainEvent;
