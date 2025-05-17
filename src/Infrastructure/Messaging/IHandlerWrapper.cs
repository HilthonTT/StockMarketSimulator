using Application.Abstractions.Messaging;
using SharedKernel;

namespace Infrastructure.Messaging;

internal interface IHandlerWrapper
{
    Task<Result> Handle(ICommand command, IServiceProvider serviceProvider, CancellationToken cancellationToken);
}

internal interface IHandlerWrapper<TResponse>
{
    Task<Result<TResponse>> Handle(
        object request, 
        IServiceProvider serviceProvider, 
        CancellationToken cancellationToken);
}
