using Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Infrastructure.Messaging;

internal sealed class QueryHandlerWrapper<TQuery, TResponse> : IHandlerWrapper<TResponse>
    where TQuery : IQuery<TResponse>
{
    public Task<Result<TResponse>> Handle(
        object request, 
        IServiceProvider serviceProvider, 
        CancellationToken cancellationToken)
    {
        var handler = serviceProvider.GetRequiredService<IQueryHandler<TQuery, TResponse>>();

        return handler.Handle((TQuery)request, cancellationToken);
    }
}