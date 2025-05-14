using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Infrastructure.DomainEvents;

internal sealed class DomainEventsDispatcher(IServiceProvider serviceProvider) : IDomainEventsDispatcher
{
    private static readonly ConcurrentDictionary<Type, Type> HandlerTypeDictionary = new();
    private static readonly ConcurrentDictionary<Type, Type> WrapperTypeDictionary = new();

    public async Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = serviceProvider.CreateScope();

        foreach (IDomainEvent domainEvent in domainEvents)
        {
            await HandleInternal(scope, domainEvent, cancellationToken);
        }
    }

    public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = serviceProvider.CreateScope();

        await HandleInternal(scope, domainEvent, cancellationToken);
    }

    private static async Task HandleInternal(
        IServiceScope scope, 
        IDomainEvent domainEvent, 
        CancellationToken cancellationToken)
    {
        Type domainEventType = domainEvent.GetType();
        Type handlerType = HandlerTypeDictionary.GetOrAdd(
            domainEventType, et => typeof(IDomainEventHandler<>).MakeGenericType(et));

        IEnumerable<object?> handlers = scope.ServiceProvider.GetServices(handlerType);

        foreach (object? handler in handlers)
        {
            if (handler is null)
            {
                continue;
            }

            HandlerWrapper? handlerWrapper = HandlerWrapper.Create(handler, domainEventType);
            if (handlerWrapper is not null)
            {
                await handlerWrapper.Handle(domainEvent, cancellationToken);
            }
        }
    }

    private abstract class HandlerWrapper
    {
        public abstract Task Handle(IDomainEvent domainEvent, CancellationToken cancellationToken = default);

        public static HandlerWrapper? Create(object handler, Type domainEventType)
        {
            Type wrapperType = WrapperTypeDictionary.GetOrAdd(
                domainEventType,
                et => typeof(HandlerWrapper<>).MakeGenericType(et));

            return (HandlerWrapper?)Activator.CreateInstance(wrapperType, handler);
        }
    }

    private sealed class HandlerWrapper<T>(object handler) : HandlerWrapper 
        where T : IDomainEvent
    {
        private readonly IDomainEventHandler<T> _handler = (IDomainEventHandler<T>)handler;

        public override async Task Handle(IDomainEvent domainEvent, CancellationToken cancellationToken = default)
        {
            await _handler.Handle((T)domainEvent, cancellationToken);
        }
    }
}
