using System.Data;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Modules.Users.Contracts.Followers;
using SharedKernel;

namespace Modules.Users.Application.Followers.GetFollowerStats;

internal sealed class GetFollowerStatsQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetFollowerStatsQuery, FollowerStatsResponse>
{
    public async Task<Result<FollowerStatsResponse>> Handle(
        GetFollowerStatsQuery query, 
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = await dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        FollowerStatsResponse followerStats = await FollowerQueries.GetFollowerStatsAsync(connection, query.UserId);

        return followerStats;
    }
}
