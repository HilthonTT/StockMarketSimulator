using SharedKernel;

namespace Infrastructure.DomainEvents;

public interface IDomainEventsDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);

    Task DispatchAsync(IDomainEvent domainEvent, CancellationToken cancellationToken = default);
}
