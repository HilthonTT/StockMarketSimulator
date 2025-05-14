using Application.Abstractions.Events;

namespace Infrastructure.Events;

public interface IIntegrationEventsDispatcher
{
    Task DispatchAsync(IIntegrationEvent integrationEvent, CancellationToken cancellationToken = default);
}
