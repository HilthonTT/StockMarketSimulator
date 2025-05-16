using Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Infrastructure.Messaging;

internal sealed class Sender(IServiceProvider serviceProvider) : ISender
{
    public Task<Result> Send<TResponse>(ICommand command, CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = serviceProvider.CreateScope();

        Type commandType = command.GetType();
        Type handlerType = typeof(ICommandHandler<>).MakeGenericType(commandType);

        dynamic handler = scope.ServiceProvider.GetRequiredService(handlerType);

        return handler.Handle((dynamic)command, cancellationToken);
    }

    public Task<Result<TResponse>> Send<TResponse>(ICommand<TResponse> command, CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = serviceProvider.CreateScope();

        Type commandType = command.GetType();
        Type handlerType = typeof(ICommandHandler<,>).MakeGenericType(commandType, typeof(TResponse));

        dynamic handler = scope.ServiceProvider.GetRequiredService(handlerType);
        return handler.Handle((dynamic)command, cancellationToken);
    }

    public Task<Result<TResponse>> Send<TResponse>(IQuery<TResponse> query, CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = serviceProvider.CreateScope();

        Type queryType = query.GetType();
        Type handlerType = typeof(IQueryHandler<,>).MakeGenericType(queryType, typeof(TResponse));

        dynamic handler = scope.ServiceProvider.GetRequiredService(handlerType);
        return handler.Handle((dynamic)query, cancellationToken);
    }
}
