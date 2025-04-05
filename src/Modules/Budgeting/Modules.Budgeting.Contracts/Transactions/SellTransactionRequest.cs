namespace Modules.Budgeting.Contracts.Transactions;

public sealed record SellTransactionRequest(Guid UserId, string Ticker, int Quantity);
