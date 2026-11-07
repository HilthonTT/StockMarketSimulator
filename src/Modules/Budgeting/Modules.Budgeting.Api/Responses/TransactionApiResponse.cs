namespace Modules.Budgeting.Api.Responses;

/// <param name="Amount">Price per share at the time of the transaction.</param>
public sealed record TransactionApiResponse(
    Guid Id,
    string Ticker,
    int Type,
    decimal Amount,
    string CurrencyCode,
    int Quantity);
