using Infrastructure.Database.Specifications;
using Microsoft.EntityFrameworkCore;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Repositories;
using Modules.Users.Infrastructure.Database;
using Modules.Users.Infrastructure.Specifications;
using SharedKernel;

namespace Modules.Users.Infrastructure.Repositories;

internal sealed class FollowerRepository(UsersDbContext context) : IFollowerRepository
{
    public async Task<Option<Follower>> GetByIdAsync(
        Guid userId, 
        Guid followedId, 
        CancellationToken cancellationToken = default)
    {
        Follower? follower = await ApplySpecification(new FollowerGetByIdSpecification(userId, followedId))
            .FirstOrDefaultAsync(cancellationToken);

        return Option<Follower>.Some(follower);
    }

    public Task<bool> IsAlreadyFollowingAsync(
        Guid userId, 
        Guid followedId, 
        CancellationToken cancellationToken = default)
    {
        return ApplySpecification(new FollowerGetByIdSpecification(userId, followedId))
            .AnyAsync(cancellationToken);
    }

    public void Insert(Follower follower)
    {
        context.Followers.Add(follower);
    }

    public void Remove(Follower follower)
    {
        context.Followers.Remove(follower);
    }

    private IQueryable<Follower> ApplySpecification(Specification<Follower> specification)
    {
        return SpecificationEvaluator.GetQuery(context.Followers, specification);
    }
}
