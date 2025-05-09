using Application.Abstractions.Messaging;
using Infrastructure.Database;
using Infrastructure.Outbox;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Infrastructure.Idempotence;

public sealed class IdempotentDomainEventHandler<TDomainEvent> : IDomainEventHandler<TDomainEvent>
    where TDomainEvent : IDomainEvent
{
    private readonly INotificationHandler<TDomainEvent> _decorated;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public IdempotentDomainEventHandler(
        INotificationHandler<TDomainEvent> decorated,
        IServiceScopeFactory serviceScopeFactory)
    {
        _decorated = decorated;
        _serviceScopeFactory = serviceScopeFactory;
    }

    public async Task Handle(TDomainEvent notification, CancellationToken cancellationToken)
    {
        using IServiceScope scope = _serviceScopeFactory.CreateScope();

        IOutboxMessageConsumerRepository outboxMessageConsumerRepository =
            scope.ServiceProvider.GetRequiredService<IOutboxMessageConsumerRepository>();

        GeneralDbContext dbContext =
            scope.ServiceProvider.GetRequiredService<GeneralDbContext>();

        string consumerName = _decorated.GetType().Name;

        if (await outboxMessageConsumerRepository.ExistsAsync(notification.Id, consumerName, cancellationToken))
        {
            return;
        }

        await _decorated.Handle(notification, cancellationToken);

        var consumerRecord = new OutboxMessageConsumer
        {
            Id = notification.Id,
            Name = consumerName
        };

        outboxMessageConsumerRepository.Insert(consumerRecord);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
