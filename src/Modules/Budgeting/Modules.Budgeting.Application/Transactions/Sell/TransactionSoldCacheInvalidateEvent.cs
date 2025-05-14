namespace Modules.Budgeting.Application.Transactions.Sell;

internal sealed record TransactionSoldCacheInvalidateEvent(Guid TransactionId, Guid UserId);
