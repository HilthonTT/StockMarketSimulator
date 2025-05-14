using Application.Abstractions.Messaging;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Application.Abstractions.Behaviors;

public static class ExceptionHandlingDecorator
{
    public sealed class CommandHandler<TCommand, TResponse>(
        ICommandHandler<TCommand, TResponse> innerHandler,
        ILogger<CommandHandler<TCommand, TResponse>> logger)
        : ICommandHandler<TCommand, TResponse>
        where TCommand : ICommand<TResponse>
    {
        public Task<Result<TResponse>> Handle(TCommand command, CancellationToken cancellationToken)
        {
            try
            {
                return innerHandler.Handle(command, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unhandled exception for {RequestName}", typeof(TCommand).Name);
                throw;
            }
        }
    }

    public sealed class CommandBaseHandler<TCommand>(
        ICommandHandler<TCommand> innerHandler,
        ILogger<CommandBaseHandler<TCommand>> logger)
        : ICommandHandler<TCommand>
        where TCommand : ICommand
    {
        public Task<Result> Handle(TCommand command, CancellationToken cancellationToken)
        {
            try
            {
                return innerHandler.Handle(command, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unhandled exception for {RequestName}", typeof(TCommand).Name);
                throw;
            }
        }
    }

    public sealed class QueryHandler<TQuery, TResponse>(
        IQueryHandler<TQuery, TResponse> innerHandler,
        ILogger<QueryHandler<TQuery, TResponse>> logger)
        : IQueryHandler<TQuery, TResponse>
        where TQuery : IQuery<TResponse>
    {
        public Task<Result<TResponse>> Handle(TQuery query, CancellationToken cancellationToken)
        {
            try
            {
                return innerHandler.Handle(query, cancellationToken);
            }
            catch (Exception exception)
            {
                logger.LogError(exception, "Unhandled exception for {RequestName}", typeof(TQuery).Name);
                throw;
            }
        }
    }
}
