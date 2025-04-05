namespace Modules.Budgeting.Api;

public sealed record BudgetApiResponse(Guid Id, Guid UserId, decimal BuyingPower);
