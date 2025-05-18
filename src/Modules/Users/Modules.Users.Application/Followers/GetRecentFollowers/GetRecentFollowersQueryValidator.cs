using FluentValidation;

namespace Modules.Users.Application.Followers.GetRecentFollowers;

internal sealed class GetRecentFollowersQueryValidator : AbstractValidator<GetRecentFollowersQuery>
{
    public GetRecentFollowersQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
