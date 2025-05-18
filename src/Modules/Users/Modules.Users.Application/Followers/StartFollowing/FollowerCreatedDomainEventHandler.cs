using Application.Abstractions.Events;
using Modules.Users.Domain.DomainEvents;
using SharedKernel;

namespace Modules.Users.Application.Followers.StartFollowing;

internal sealed class FollowerCreatedDomainEventHandler(IEventBus eventBus) 
    : IDomainEventHandler<FollowerCreatedDomainEvent>
{
    public Task Handle(FollowerCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        // TODO: Add email notification
        return Task.CompletedTask;
    }
}
