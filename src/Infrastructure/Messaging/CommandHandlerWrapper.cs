using Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Infrastructure.Messaging;

internal sealed class CommandHandlerWrapper<TCommand> : IHandlerWrapper
    where TCommand : ICommand
{
    public Task<Result> Handle(
        ICommand command, 
        IServiceProvider serviceProvider, 
        CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand>>();

        return handler.Handle((TCommand)command, cancellationToken);
    }
}

internal sealed class CommandHandlerWrapper<TCommand, TResponse> : IHandlerWrapper<TResponse>
    where TCommand : ICommand<TResponse>
{
    public Task<Result<TResponse>> Handle(
        object request, 
        IServiceProvider serviceProvider, 
        CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetRequiredService<ICommandHandler<TCommand, TResponse>>();

        return handler.Handle((TCommand)request, cancellationToken);
    }
}
