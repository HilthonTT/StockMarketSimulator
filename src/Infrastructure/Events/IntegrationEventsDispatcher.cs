using System.Collections.Concurrent;
using Application.Abstractions.Events;
using Application.Abstractions.Messaging;
using EntityFramework.Exceptions.Common;
using Infrastructure.Database;
using Infrastructure.Outbox;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Events;

/// <summary>
/// Dispatches an integration event to its in-process handlers, at most once per (event id, handler).
/// </summary>
/// <remarks>
/// Delivery is at-least-once: a message is redelivered when any handler fails, and a publisher can publish
/// the same event twice. Each successful handler is recorded in <c>general.outbox_message_consumers</c>
/// (the table that also deduplicates domain-event handlers) under <c>integration:{HandlerName}</c>, so a
/// redelivery only runs the handlers that haven't completed yet.
/// </remarks>
internal sealed class IntegrationEventsDispatcher(IServiceProvider serviceProvider) : IIntegrationEventsDispatcher
{
    private static readonly ConcurrentDictionary<Type, Type> HandlerTypeDictionary = new();
    private static readonly ConcurrentDictionary<Type, Type> WrapperTypeDictionary = new();

    public async Task DispatchAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = serviceProvider.CreateScope();

        Type integrationEventType = integrationEvent.GetType();
        Type handlerType = HandlerTypeDictionary.GetOrAdd(
            integrationEventType,
            et => typeof(IIntegrationEventHandler<>).MakeGenericType(et));

        IEnumerable<object?> handlers = scope.ServiceProvider.GetServices(handlerType);

        foreach (object? handler in handlers)
        {
            if (handler is null)
            {
                continue;
            }

            HandlerWrapper? handlerWrapper = HandlerWrapper.Create(handler, integrationEventType);
            if (handlerWrapper is null)
            {
                continue;
            }

            string consumerName = CreateConsumerName(handler.GetType());

            await HandleOnceAsync(integrationEvent, handlerWrapper, consumerName, cancellationToken);
        }
    }

    internal static string CreateConsumerName(Type handlerType) => $"integration:{handlerType.Name}";

    private async Task HandleOnceAsync(
        IIntegrationEvent integrationEvent,
        HandlerWrapper handlerWrapper,
        string consumerName,
        CancellationToken cancellationToken)
    {
        // A fresh scope per record so a failed save can't leak tracked entities into the next handler.
        using IServiceScope scope = serviceProvider.CreateScope();

        IOutboxMessageConsumerRepository consumers =
            scope.ServiceProvider.GetRequiredService<IOutboxMessageConsumerRepository>();

        if (await consumers.ExistsAsync(integrationEvent.Id, consumerName, cancellationToken))
        {
            return;
        }

        await handlerWrapper.Handle(integrationEvent, cancellationToken);

        consumers.Insert(new OutboxMessageConsumer { Id = integrationEvent.Id, Name = consumerName });

        try
        {
            await scope.ServiceProvider.GetRequiredService<GeneralDbContext>().SaveChangesAsync(cancellationToken);
        }
        catch (UniqueConstraintException)
        {
            // Another instance handled and recorded the same delivery concurrently.
        }
    }

    private abstract class HandlerWrapper
    {
        public abstract Task Handle(IIntegrationEvent domainEvent, CancellationToken cancellationToken);

        public static HandlerWrapper? Create(object handler, Type integrationEventType)
        {
            Type wrapperType = WrapperTypeDictionary.GetOrAdd(
                integrationEventType,
                et => typeof(HandlerWrapper<>).MakeGenericType(et));

            return (HandlerWrapper?)Activator.CreateInstance(wrapperType, handler);
        }
    }

    private sealed class HandlerWrapper<T>(object handler) : HandlerWrapper where T : IIntegrationEvent
    {
        private readonly IIntegrationEventHandler<T> _handler = (IIntegrationEventHandler<T>)handler;

        public override async Task Handle(IIntegrationEvent integrationEvent, CancellationToken cancellationToken)
        {
            await _handler.Handle((T)integrationEvent, cancellationToken);
        }
    }
}
