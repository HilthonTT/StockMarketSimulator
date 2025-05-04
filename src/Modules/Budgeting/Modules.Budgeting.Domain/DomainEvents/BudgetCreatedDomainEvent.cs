using SharedKernel;

namespace Modules.Budgeting.Domain.DomainEvents;

public sealed record BudgetCreatedDomainEvent(Guid Id, Guid BudgetId) : DomainEvent(Id);
