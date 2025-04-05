namespace Modules.Budgeting.Contracts.Transactions;

public sealed record BuyTransactionRequest(Guid UserId, string Ticker, int Quantity);
