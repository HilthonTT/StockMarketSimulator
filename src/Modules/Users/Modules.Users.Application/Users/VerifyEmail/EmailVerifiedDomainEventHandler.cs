using Application.Abstractions.Events;
using Application.Abstractions.Messaging;
using Modules.Users.Domain.DomainEvents;

namespace Modules.Users.Application.Users.VerifyEmail;

internal sealed class EmailVerifiedDomainEventHandler(IEventBus eventBus) : IDomainEventHandler<UserEmailVerifiedDomainEvent>
{
    public Task Handle(UserEmailVerifiedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new EmailVerifiedIntegrationEvent(Guid.CreateVersion7(), notification.UserId);

        return eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
