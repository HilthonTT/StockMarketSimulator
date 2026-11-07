using Modules.Users.Domain.DomainEvents;
using SharedKernel;

namespace Modules.Users.Application.Followers.StartFollowing;

/// <summary>
/// Placeholder for follower notifications. Nothing consumes a "follower created" integration event yet,
/// so nothing is published; add one (and its consumer) when follower emails are implemented.
/// </summary>
internal sealed class FollowerCreatedDomainEventHandler : IDomainEventHandler<FollowerCreatedDomainEvent>
{
    public Task Handle(FollowerCreatedDomainEvent domainEvent, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
