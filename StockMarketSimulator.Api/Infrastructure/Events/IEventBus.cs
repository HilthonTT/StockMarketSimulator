using SharedKernel;

namespace StockMarketSimulator.Api.Infrastructure.Events;

public interface IEventBus
{
    Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : class, IIntegrationEvent;

    Task<Result<TResponse>> SendAsync<TRequest, TResponse>(
        TRequest request, 
        CancellationToken cancellationToken = default) 
        where TRequest : class;
}
