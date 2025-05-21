using System.Data;
using Dapper;
using Modules.Users.Contracts.Followers;

namespace Modules.Users.Application.Followers;

internal static class FollowerQueries
{
    internal static async Task<FollowerStatsResponse> GetFollowerStatsAsync(
        IDbConnection connection,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT
                @UserId AS UserId,
                (
                    SELECT COUNT(*)
                    FROM users.followers f
                    WHERE f.followed_id = @UserId
                ) AS FollowerCount,
                (
                    SELECT COUNT(*)
                    FROM users.followers f
                    WHERE f.user_id = @UserId
                ) AS FollowingCount
            """;

        FollowerStatsResponse followerStats = await connection.QueryFirstAsync<FollowerStatsResponse>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));

        return followerStats;
    }

    internal static async Task<List<FollowerResponse>> GetRecentFollowersAsync(
        IDbConnection connection,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT 
                u.Id AS Id,
                u.name AS Name
            FROM users.followers f
            JOIN users.users u on u.id = f.user_id
            WHERE f.followed_id = @UserId
            ORDER BY created_on_utc DESC
            LIMIT @Limit
            """;

        IEnumerable<FollowerResponse> followers = await connection.QueryAsync<FollowerResponse>(
            new CommandDefinition(sql, new
            {
                UserId = userId,
                Limit = 10
            }, 
            cancellationToken: cancellationToken));

        return [.. followers];
    }
}
