using Application.Abstractions.Events;
using Application.Abstractions.Messaging;
using Modules.Users.Domain.DomainEvents;

namespace Modules.Users.Application.Authentication.ChangePassword;

internal sealed class UserPasswordChangedDomainEventHandler(IEventBus eventBus) 
    : IDomainEventHandler<UserPasswordChangedDomainEvent>
{
    public Task Handle(UserPasswordChangedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new UserPasswordChangedIntegrationEvent(Guid.CreateVersion7(), notification.UserId);

        return eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
