using Application.Core.Extensions;
using FluentValidation;
using Modules.Users.Application.Core.Errors;

namespace Modules.Users.Application.Followers.StartFollowing;

internal sealed class StartFollowingCommandValidator : AbstractValidator<StartFollowingCommand>
{
    public StartFollowingCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithError(FollowersValidationErrors.StartFollowing.UserIdIsRequired);

        RuleFor(x => x.FollowedId)
            .NotEmpty().WithError(FollowersValidationErrors.StartFollowing.FollowedIdIsRequired);
    }
}
