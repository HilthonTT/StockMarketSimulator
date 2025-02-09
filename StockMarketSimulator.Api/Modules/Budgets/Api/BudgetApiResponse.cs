namespace StockMarketSimulator.Api.Modules.Budgets.Api;

public sealed record BudgetApiResponse(Guid Id, Guid UserId, decimal BuyingPower);
