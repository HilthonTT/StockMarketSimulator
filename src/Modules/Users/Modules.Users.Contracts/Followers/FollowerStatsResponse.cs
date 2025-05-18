namespace Modules.Users.Contracts.Followers;

public sealed class FollowerStatsResponse
{
    public Guid UserId { get; init; }

    public int FollowerCount { get; init; }

    public int FollowingCount { get; init; }
}
