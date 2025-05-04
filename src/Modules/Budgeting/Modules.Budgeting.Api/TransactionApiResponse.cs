namespace Modules.Budgeting.Api;

public sealed record TransactionApiResponse(Guid Id, string Ticker, int Type, decimal LimitPrice);
