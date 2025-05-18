using System.Data;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Modules.Users.Contracts.Followers;
using SharedKernel;

namespace Modules.Users.Application.Followers.GetRecentFollowers;

internal sealed class GetRecentFollowersQueryHandler(IDbConnectionFactory dbConnectionFactory) 
    : IQueryHandler<GetRecentFollowersQuery, List<FollowerResponse>>
{
    public async Task<Result<List<FollowerResponse>>> Handle(
        GetRecentFollowersQuery query, 
        CancellationToken cancellationToken)
    {
        using IDbConnection connection = await dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        List<FollowerResponse> followers = await FollowerQueries.GetRecentFollowersAsync(connection, query.UserId);

        return followers;
    }
}
