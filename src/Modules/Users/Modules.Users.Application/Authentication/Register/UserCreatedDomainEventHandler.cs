using Application.Abstractions.Events;
using Application.Abstractions.Messaging;
using Modules.Users.Domain.DomainEvents;

namespace Modules.Users.Application.Authentication.Register;

internal sealed class UserCreatedDomainEventHandler(IEventBus eventBus) : IDomainEventHandler<UserCreatedDomainEvent>
{
    public Task Handle(UserCreatedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new UserCreatedIntegrationEvent(
            Guid.CreateVersion7(),
            notification.UserId, 
            notification.VerificationLink);

        return eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
