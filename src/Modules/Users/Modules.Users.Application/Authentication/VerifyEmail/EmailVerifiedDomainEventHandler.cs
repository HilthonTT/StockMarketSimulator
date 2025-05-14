using Application.Abstractions.Events;
using Modules.Users.Domain.DomainEvents;
using SharedKernel;

namespace Modules.Users.Application.Authentication.VerifyEmail;

internal sealed class EmailVerifiedDomainEventHandler(IEventBus eventBus) : IDomainEventHandler<UserEmailVerifiedDomainEvent>
{
    public Task Handle(UserEmailVerifiedDomainEvent notification, CancellationToken cancellationToken)
    {
        var integrationEvent = new EmailVerifiedIntegrationEvent(Guid.CreateVersion7(), notification.UserId);

        return eventBus.PublishAsync(integrationEvent, cancellationToken);
    }
}
