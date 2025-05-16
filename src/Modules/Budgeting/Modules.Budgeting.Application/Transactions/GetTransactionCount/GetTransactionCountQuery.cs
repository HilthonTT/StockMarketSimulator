using Application.Abstractions.Caching;
using Modules.Budgeting.Contracts.Transactions;

namespace Modules.Budgeting.Application.Transactions.GetTransactionCount;

public sealed record GetTransactionCountQuery(Guid UserId) : ICachedQuery<TransactionCountResponse>
{
    public string CacheKey => $"users:{UserId}:purchased-stock-tickers";

    public TimeSpan? Expiration => null;
}
