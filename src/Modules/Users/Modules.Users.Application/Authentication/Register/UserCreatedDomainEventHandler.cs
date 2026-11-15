using Modules.Users.IntegrationEvents;
using Application.Abstractions.Events;
using Modules.Users.Domain.DomainEvents;
using SharedKernel;

namespace Modules.Users.Application.Authentication.Register;

internal sealed class UserCreatedDomainEventHandler(IEventBus eventBus) : IDomainEventHandler<UserCreatedDomainEvent>
{
    public Task Handle(UserCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        // Reuse the domain event's id: a retried publish then carries the same id and consumers dedupe it.
        var integrationEvent = new UserCreatedIntegrationEvent(
            notification.Id,
            notification.UserId, 
            notification.VerificationLink);

        return eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
