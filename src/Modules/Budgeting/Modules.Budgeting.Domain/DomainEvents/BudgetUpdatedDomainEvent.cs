using SharedKernel;

namespace Modules.Budgeting.Domain.DomainEvents;

public sealed record BudgetUpdatedDomainEvent(Guid Id, Guid BudgetId, decimal NewBuyingPower) : DomainEvent(Id);
