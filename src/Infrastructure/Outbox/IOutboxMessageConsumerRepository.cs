namespace Infrastructure.Outbox;

public interface IOutboxMessageConsumerRepository
{
    Task<bool> ExistsAsync(Guid notificationId, string consumerName, CancellationToken cancellationToken = default);

    void Insert(OutboxMessageConsumer consumer);
}
