using Modules.Users.Domain.Entities;
using SharedKernel;

namespace Modules.Users.Domain.Services;

public interface IFollowerService
{
    Task<Result<Follower>> StartFollowingAsync(User user, User followed, CancellationToken cancellationToken = default);

    Task<Result<Follower>> StopFollowingAsync(User user, User followed, CancellationToken cancellationToken = default);
}
