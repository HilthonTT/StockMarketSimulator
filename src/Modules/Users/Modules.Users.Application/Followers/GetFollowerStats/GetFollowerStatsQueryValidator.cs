using FluentValidation;

namespace Modules.Users.Application.Followers.GetFollowerStats;

internal sealed class GetFollowerStatsQueryValidator : AbstractValidator<GetFollowerStatsQuery>
{
    public GetFollowerStatsQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
