using Infrastructure.Database.Specifications;
using Modules.Users.Domain.Entities;

namespace Modules.Users.Infrastructure.Specifications;

internal sealed class FollowerGetByIdSpecification : Specification<Follower>
{
    public FollowerGetByIdSpecification(Guid userId, Guid followedId) 
        : base(follower => follower.UserId == userId && follower.FollowedId == followedId)
    {
    }
}
