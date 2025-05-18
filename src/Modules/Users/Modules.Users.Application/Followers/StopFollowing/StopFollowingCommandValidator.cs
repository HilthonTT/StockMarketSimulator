using Application.Core.Extensions;
using FluentValidation;
using Modules.Users.Application.Core.Errors;

namespace Modules.Users.Application.Followers.StopFollowing;

internal sealed class StopFollowingCommandValidator : AbstractValidator<StopFollowingCommand>
{
    public StopFollowingCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithError(FollowersValidationErrors.StopFollowing.UserIdIsRequired);

        RuleFor(x => x.FollowedId)
            .NotEmpty().WithError(FollowersValidationErrors.StopFollowing.FollowedIdIsRequired);
    }
}
