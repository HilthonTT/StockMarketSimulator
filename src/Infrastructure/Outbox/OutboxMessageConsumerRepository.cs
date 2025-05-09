using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Outbox;

internal sealed class OutboxMessageConsumerRepository(GeneralDbContext context) : IOutboxMessageConsumerRepository
{
    public Task<bool> ExistsAsync(Guid notificationId, string consumerName, CancellationToken cancellationToken = default)
    {
        return context.OutboxMessageConsumers
            .AnyAsync(consumer =>
                consumer.Id == notificationId &&
                consumer.Name == consumerName,
                cancellationToken);
    }

    public void Insert(OutboxMessageConsumer consumer)
    {
        context.OutboxMessageConsumers.Add(consumer);
    }
}
