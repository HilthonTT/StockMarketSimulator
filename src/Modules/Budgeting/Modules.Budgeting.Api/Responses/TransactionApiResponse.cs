namespace Modules.Budgeting.Api.Responses;

public sealed record TransactionApiResponse(Guid Id, string Ticker, int Type, decimal Amount, string CurrencyCode);
