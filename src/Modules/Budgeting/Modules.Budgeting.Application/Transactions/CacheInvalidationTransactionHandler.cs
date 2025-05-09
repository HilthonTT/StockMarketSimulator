using Application.Abstractions.Caching;
using Modules.Budgeting.Application.Transactions.Buy;
using Modules.Budgeting.Application.Transactions.Sell;

namespace Modules.Budgeting.Application.Transactions;

internal sealed class CacheInvalidationTransactionHandler :
    ICacheInvalidateEventHandler<TransactionBoughtCacheInvalidateEvent>,
    ICacheInvalidateEventHandler<TransactionSoldCacheInvalidateEvent>
{
    private readonly ICacheService _cacheService;

    public CacheInvalidationTransactionHandler(ICacheService cacheService)
    {
        _cacheService = cacheService;
    }

    public Task Handle(TransactionBoughtCacheInvalidateEvent notification, CancellationToken cancellationToken)
    {
        return HandleInternal(notification.TransactionId, notification.UserId, cancellationToken);
    }

    public Task Handle(TransactionSoldCacheInvalidateEvent notification, CancellationToken cancellationToken)
    {
        return HandleInternal(notification.TransactionId, notification.UserId, cancellationToken);
    }

    private Task HandleInternal(Guid transactionId, Guid userId, CancellationToken cancellationToken)
    {
        return _cacheService.RemoveAsync($"users:{userId}:purchased-stock-tickers", cancellationToken);
    }
}
