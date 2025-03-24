namespace StockMarketSimulator.Api.Modules.Budgets.Contracts;

internal sealed record BudgetResponse(Guid Id, Guid UserId, decimal BuyingPower);
