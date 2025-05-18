using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Modules.Users.Application.Abstractions.Data;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Repositories;
using Modules.Users.Domain.Services;
using SharedKernel;

namespace Modules.Users.Application.Followers.StopFollowing;

internal sealed class StopFollowingCommandHandler(
    IUserRepository userRepository,
    IFollowerService followerService,
    IFollowerRepository followerRepository,
    IUnitOfWork unitOfWork,
    IUserContext userContext) : ICommandHandler<StopFollowingCommand>
{
    public async Task<Result> Handle(StopFollowingCommand command, CancellationToken cancellationToken)
    {
        if (userContext.UserId != command.UserId)
        {
            return Result.Failure(UserErrors.Unauthorized);
        }

        Option<User> optionUser = await userRepository.GetByIdAsync(command.UserId, cancellationToken);
        if (!optionUser.IsSome)
        {
            return Result.Failure(UserErrors.NotFound(command.UserId));
        }

        User user = optionUser.ValueOrThrow();

        Option<User> optionFollowed = await userRepository.GetByIdAsync(command.FollowedId, cancellationToken);
        if (!optionFollowed.IsSome)
        {
            return Result.Failure(UserErrors.NotFound(command.FollowedId));
        }

        User followed = optionFollowed.ValueOrThrow();

        Result<Follower> result = await followerService.StopFollowingAsync(user, followed, cancellationToken);
        if (result.IsFailure)
        {
            return result;
        }

        followerRepository.Remove(result.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
