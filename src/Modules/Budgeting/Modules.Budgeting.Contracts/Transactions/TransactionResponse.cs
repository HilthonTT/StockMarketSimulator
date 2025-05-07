namespace Modules.Budgeting.Contracts.Transactions;

public sealed record TransactionResponse(
    Guid Id, 
    Guid UserId, 
    string Ticker, 
    decimal Amount,
    string CurrencyCode,
    int Type, 
    int Quantity,
    DateTime CreatedOnUtc,
    DateTime? ModifiedOnUtc);
