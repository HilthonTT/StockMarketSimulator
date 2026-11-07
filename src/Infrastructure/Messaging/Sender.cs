using System.Collections.Concurrent;
using Application.Abstractions.Messaging;
using SharedKernel;

namespace Infrastructure.Messaging;

internal sealed class Sender(IServiceProvider serviceProvider) : ISender
{
    private static readonly ConcurrentDictionary<Type, Type> HandlerTypeDictionary = new();

    public Task<Result> Send(ICommand command, CancellationToken cancellationToken = default)
    {
        Type commandType = command.GetType();
        Type wrapperType = HandlerTypeDictionary.GetOrAdd(
            commandType,
            static type => typeof(CommandHandlerWrapper<>).MakeGenericType(type));

        IHandlerWrapper? wrapper = (IHandlerWrapper?)Activator.CreateInstance(wrapperType);
        Ensure.NotNull(wrapper, nameof(wrapper));

        return wrapper.Handle(command, serviceProvider, cancellationToken);
    }

    public Task<Result<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        Type commandType = command.GetType();
        Type wrapperType = HandlerTypeDictionary.GetOrAdd(
            commandType,
            static type => typeof(CommandHandlerWrapper<,>).MakeGenericType(type, typeof(TResponse)));

        IHandlerWrapper<TResponse>? wrapper = (IHandlerWrapper<TResponse>?)Activator.CreateInstance(wrapperType);
        Ensure.NotNull(wrapper, nameof(wrapper));

        return wrapper.Handle(command, serviceProvider, cancellationToken);
    }

    public Task<Result<TResponse>> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        Type queryType = query.GetType();
        Type wrapperType = HandlerTypeDictionary.GetOrAdd(
            queryType,
            static type => typeof(QueryHandlerWrapper<,>).MakeGenericType(type, typeof(TResponse)));

        IHandlerWrapper<TResponse>? wrapper = (IHandlerWrapper<TResponse>?)Activator.CreateInstance(wrapperType);
        Ensure.NotNull(wrapper, nameof(wrapper));

        return wrapper.Handle(query, serviceProvider, cancellationToken);
    }
}
