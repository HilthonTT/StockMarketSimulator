namespace Modules.Budgeting.Application.Transactions.Buy;

internal sealed record TransactionBoughtCacheInvalidateEvent(Guid TransactionId, Guid UserId);
