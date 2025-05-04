using Application.Abstractions.Messaging;
using Infrastructure.Database;
using Infrastructure.Outbox;
using MediatR;
using Microsoft.EntityFrameworkCore;
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
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<GeneralDbContext>();

        string consumerName = _decorated.GetType().Name;

        bool alreadyHandled = await dbContext.OutboxMessageConsumers
            .AnyAsync(consumer =>
                consumer.Id == notification.Id &&
                consumer.Name == consumerName,
                cancellationToken);

        if (alreadyHandled)
        {
            return;
        }

        await _decorated.Handle(notification, cancellationToken);

        var consumerRecord = new OutboxMessageConsumer
        {
            Id = notification.Id,
            Name = consumerName
        };

        dbContext.OutboxMessageConsumers.Add(consumerRecord);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
