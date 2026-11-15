using Modules.Users.IntegrationEvents;
using Application.Abstractions.Events;
using Modules.Users.Domain.DomainEvents;
using SharedKernel;

namespace Modules.Users.Application.Authentication.ChangePassword;

internal sealed class UserPasswordChangedDomainEventHandler(IEventBus eventBus) 
    : IDomainEventHandler<UserPasswordChangedDomainEvent>
{
    public Task Handle(UserPasswordChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        // Reuse the domain event's id: a retried publish then carries the same id and consumers dedupe it.
        var integrationEvent = new UserPasswordChangedIntegrationEvent(notification.Id, notification.UserId);

        return eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
