using Application.Abstractions.Caching;
using Modules.Budgeting.Application.Transactions.Buy;
using Modules.Budgeting.Application.Transactions.Sell;

namespace Modules.Budgeting.Application.Transactions;

internal static class TransactionCacheInvalidator
{
    public static Task InvalidateAsync(
        ICacheService cacheService, 
        TransactionBoughtCacheInvalidateEvent @event,
        CancellationToken cancellationToken = default)
    {
        return HandleInternal(cacheService, @event.TransactionId, @event.UserId, cancellationToken);
    }

    public static Task InvalidateAsync(
        ICacheService cacheService,
        TransactionSoldCacheInvalidateEvent @event,
        CancellationToken cancellationToken = default)
    {
        return HandleInternal(cacheService, @event.TransactionId, @event.UserId, cancellationToken);
    }

    private static Task HandleInternal(
        ICacheService cacheService,
        Guid transactionId, 
        Guid userId, 
        CancellationToken cancellationToken)
    {
        return Task.WhenAll(
            cacheService.RemoveAsync($"users:{userId}:purchased-stock-tickers", cancellationToken),
            cacheService.RemoveAsync($"users:{userId}:transaction-count", cancellationToken));
    }
}
