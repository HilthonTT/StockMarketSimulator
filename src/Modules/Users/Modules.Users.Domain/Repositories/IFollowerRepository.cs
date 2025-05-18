using Modules.Users.Domain.Entities;
using SharedKernel;

namespace Modules.Users.Domain.Repositories;

public interface IFollowerRepository
{
    Task<bool> IsAlreadyFollowingAsync(Guid userId, Guid followedId, CancellationToken cancellationToken = default);

    Task<Option<Follower>> GetByIdAsync(Guid userId, Guid followedId, CancellationToken cancellationToken = default);

    void Insert(Follower follower);

    void Remove(Follower follower);
}
