using FluentValidation;

namespace Modules.Users.Application.Leaderboard;

internal sealed class GetLeaderboardQueryValidator : AbstractValidator<GetLeaderboardQuery>
{
    public GetLeaderboardQueryValidator()
    {
        RuleFor(x => x.Limit).InclusiveBetween(1, 100);
    }
}
