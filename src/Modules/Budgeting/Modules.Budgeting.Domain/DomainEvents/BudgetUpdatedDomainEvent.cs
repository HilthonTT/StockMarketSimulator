using SharedKernel;

namespace Modules.Budgeting.Domain.DomainEvents;

public sealed record BudgetUpdatedDomainEvent(Guid BudgetId, decimal NewBuyingPower) : IDomainEvent;
