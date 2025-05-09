using Application.Abstractions.Caching;

namespace Infrastructure.Outbox;

internal sealed class CachedOutboxMessageConsumerRepository(
    IOutboxMessageConsumerRepository decorated,
    ICacheService cacheService) : IOutboxMessageConsumerRepository
{
    public async Task<bool> ExistsAsync(
        Guid notificationId, 
        string consumerName, 
        CancellationToken cancellationToken = default)
    {
        string key = $"outbox_message_consumer:{notificationId}:{consumerName}";

        var cached = await cacheService.GetAsync<bool?>(key, cancellationToken);
        if (cached.HasValue)
        {
            return cached.Value;
        }

        bool exists = await decorated.ExistsAsync(notificationId, consumerName, cancellationToken);

        await cacheService.SetAsync(key, exists, TimeSpan.FromMinutes(5), cancellationToken);

        return exists;
    }

    public void Insert(OutboxMessageConsumer consumer)
    {
        decorated.Insert(consumer);
    }
}
