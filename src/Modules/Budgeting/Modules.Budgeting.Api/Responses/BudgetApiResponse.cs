namespace Modules.Budgeting.Api.Responses;

public sealed record BudgetApiResponse(Guid Id, Guid UserId, decimal BuyingPower);
