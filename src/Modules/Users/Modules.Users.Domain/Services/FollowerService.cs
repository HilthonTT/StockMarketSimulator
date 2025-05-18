using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Repositories;
using SharedKernel;

namespace Modules.Users.Domain.Services;

public sealed class FollowerService(
    IFollowerRepository followerRepository,
    IDateTimeProvider dateTimeProvider) : IFollowerService
{
    public async Task<Result<Follower>> StartFollowingAsync(
        User user, 
        User followed, 
        CancellationToken cancellationToken = default)
    {
        if (user.Id == followed.Id)
        {
            return Result.Failure<Follower>(FollowerErrors.SameUser);
        }

        if (!followed.HasPublicProfile)
        {
            return Result.Failure<Follower>(FollowerErrors.NonPublicProfile);
        }

        if (await followerRepository.IsAlreadyFollowingAsync(user.Id, followed.Id, cancellationToken))
        {
            return Result.Failure<Follower>(FollowerErrors.AlreadyFollowing);
        }

        var follower = Follower.Create(user.Id, followed.Id, dateTimeProvider.UtcNow);

        return follower;
    }

    public async Task<Result<Follower>> StopFollowingAsync(
        User user, 
        User followed, 
        CancellationToken cancellationToken = default)
    {
        Option<Follower> optionFollower = await followerRepository.GetByIdAsync(user.Id, followed.Id, cancellationToken);
        if (!optionFollower.IsSome)
        {
            return Result.Failure<Follower>(FollowerErrors.NotFollowing);
        }

        Follower follower = optionFollower.ValueOrThrow();

        return follower;
    }
}
