using Modules.Users.IntegrationEvents;
using Application.Abstractions.Events;
using Modules.Users.Domain.DomainEvents;
using SharedKernel;

namespace Modules.Users.Application.Authentication.VerifyEmail;

internal sealed class EmailVerifiedDomainEventHandler(IEventBus eventBus) : IDomainEventHandler<UserEmailVerifiedDomainEvent>
{
    public Task Handle(UserEmailVerifiedDomainEvent notification, CancellationToken cancellationToken)
    {
        // Reuse the domain event's id: a retried publish then carries the same id and consumers dedupe it.
        var integrationEvent = new EmailVerifiedIntegrationEvent(notification.Id, notification.UserId);

        return eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
