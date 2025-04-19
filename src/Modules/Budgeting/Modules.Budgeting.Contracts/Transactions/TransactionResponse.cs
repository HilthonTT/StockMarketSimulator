using Modules.Budgeting.Domain.Enums;

namespace Modules.Budgeting.Contracts.Transactions;

public sealed record TransactionResponse(
    Guid Id, 
    Guid UserId, 
    string Ticker, 
    decimal LimitPrice, 
    TransactionType Type, 
    int Quantity,
    DateTime CreatedOnUtc,
    DateTime? ModifiedOnUtc);
