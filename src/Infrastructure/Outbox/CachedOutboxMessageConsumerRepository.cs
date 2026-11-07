using Application.Abstractions.Caching;

namespace Infrastructure.Outbox;

/// <summary>
/// Caches positive "already consumed" lookups.
/// </summary>
/// <remarks>
/// Only <c>true</c> results are cached. Caching a negative result would make the idempotency
/// check report "not consumed" for the cache lifetime even after the consumer record was written,
/// which allows the same domain event to be handled twice when it is retried.
/// </remarks>
internal sealed class CachedOutboxMessageConsumerRepository(
    IOutboxMessageConsumerRepository decorated,
    ICacheService cacheService) : IOutboxMessageConsumerRepository
{
    private static readonly TimeSpan Expiration = TimeSpan.FromMinutes(5);

    public async Task<bool> ExistsAsync(
        Guid notificationId,
        string consumerName,
        CancellationToken cancellationToken = default)
    {
        string key = CreateKey(notificationId, consumerName);

        bool? cached = await cacheService.GetAsync<bool?>(key, cancellationToken);
        if (cached is true)
        {
            return true;
        }

        bool exists = await decorated.ExistsAsync(notificationId, consumerName, cancellationToken);

        if (exists)
        {
            await cacheService.SetAsync(key, true, Expiration, cancellationToken);
        }

        return exists;
    }

    public void Insert(OutboxMessageConsumer consumer)
    {
        decorated.Insert(consumer);
    }

    private static string CreateKey(Guid notificationId, string consumerName) =>
        $"outbox_message_consumer:{notificationId}:{consumerName}";
}
