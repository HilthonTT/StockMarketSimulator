using MassTransit;
using SharedKernel;

namespace StockMarketSimulator.Api.Infrastructure.Events;

internal sealed class EventBus : IEventBus
{
    private readonly IPublishEndpoint _publishEndpoint;
    private readonly IServiceProvider _serviceProvider;

    public EventBus(
        IPublishEndpoint publishEndpoint,
        IServiceProvider serviceProvider)
    {
        _publishEndpoint = publishEndpoint;
        _serviceProvider = serviceProvider;
    }

    public async Task PublishAsync<T>(T integrationEvent, CancellationToken cancellationToken = default)
        where T : class, IIntegrationEvent
    {
        await _publishEndpoint.Publish(integrationEvent, cancellationToken);
    }

    public async Task<Result<TResponse>> SendAsync<TRequest, TResponse>(
        TRequest request,
        CancellationToken cancellationToken = default) 
        where TRequest : class
    {
        IRequestClient<TRequest> client = _serviceProvider.GetRequiredService<IRequestClient<TRequest>>();

        Response<Result<TResponse>> response = await client.GetResponse<Result<TResponse>>(request, cancellationToken);

        return response.Message;
    }
}
