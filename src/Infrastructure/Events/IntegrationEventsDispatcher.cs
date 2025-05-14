using System.Collections.Concurrent;
using Application.Abstractions.Events;
using Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Events;

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
            if (handlerWrapper is not null)
            {
                await handlerWrapper.Handle(integrationEvent, cancellationToken);
            }
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
