using SharedKernel;

namespace Modules.Budgeting.Domain.DomainEvents;

public sealed record BudgetCreatedDomainEvent(Guid BudgetId) : IDomainEvent;
