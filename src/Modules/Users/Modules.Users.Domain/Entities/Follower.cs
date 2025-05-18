using System.Runtime.InteropServices;
using Modules.Users.Domain.DomainEvents;
using SharedKernel;

namespace Modules.Users.Domain.Entities;

public sealed class Follower : Entity
{
    private Follower(Guid userId, Guid followedId, DateTime createdOnUtc)
    {
        Ensure.NotNullOrEmpty(userId, nameof(userId));
        Ensure.NotNullOrEmpty(followedId, nameof(followedId));
        Ensure.NotNull(createdOnUtc, nameof(createdOnUtc));

        UserId = userId;
        FollowedId = followedId;
        CreatedOnUtc = createdOnUtc;
    }

    private Follower()
    {
    }

    public Guid UserId { get; private set; }

    public Guid FollowedId { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    public static Follower Create(Guid userId, Guid followedId, DateTime createdOnUtc)
    {
        var follower = new Follower(userId, followedId, createdOnUtc);

        follower.Raise(new FollowerCreatedDomainEvent(Guid.CreateVersion7(), userId, followedId));

        return follower;
    }
}
