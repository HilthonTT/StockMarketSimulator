namespace Modules.Budgeting.Contracts.Budgets;

public sealed record BudgetResponse(Guid Id, Guid UserId, decimal BuyingPower);
