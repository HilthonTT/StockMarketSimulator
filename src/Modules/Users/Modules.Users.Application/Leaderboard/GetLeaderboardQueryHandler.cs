using System.Data;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Dapper;
using Modules.Budgeting.Api.Api;
using Modules.Budgeting.Api.Responses;
using Modules.Users.Contracts.Leaderboard;
using SharedKernel;

namespace Modules.Users.Application.Leaderboard;

/// <summary>Ranks every account by value at current prices; ties go to the earlier account.</summary>
internal sealed class GetLeaderboardQueryHandler(
    IBudgetingApi budgetingApi,
    IDbConnectionFactory dbConnectionFactory,
    IUserContext userContext) : IQueryHandler<GetLeaderboardQuery, LeaderboardResponse>
{
    public async Task<Result<LeaderboardResponse>> Handle(GetLeaderboardQuery request, CancellationToken cancellationToken)
    {
        List<AccountValueApiResponse> accounts = await budgetingApi.GetAccountValuesAsync(cancellationToken);

        List<AccountValueApiResponse> ranked = [.. accounts
            .OrderByDescending(a => a.AccountValue)
            .ThenBy(a => a.UserId)];

        Guid currentUserId = userContext.UserId;

        HashSet<Guid> wanted = [.. ranked.Take(request.Limit).Select(a => a.UserId), currentUserId];

        using IDbConnection connection = await dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        Dictionary<Guid, string> usernames = (await connection.QueryAsync<(Guid Id, string Username)>(
                new CommandDefinition(
                    "SELECT id, username FROM users.users WHERE id = ANY(@Ids);",
                    new { Ids = wanted.ToArray() },
                    cancellationToken: cancellationToken)))
            .ToDictionary(u => u.Id, u => u.Username);

        LeaderboardEntryResponse ToEntry(AccountValueApiResponse account, int index) => new(
            index + 1,
            account.UserId,
            usernames.GetValueOrDefault(account.UserId, "Unknown trader"),
            account.AccountValue,
            account.Cash,
            account.HoldingsValue,
            account.ReturnPercent,
            account.Positions);

        List<LeaderboardEntryResponse> entries = [.. ranked.Take(request.Limit).Select(ToEntry)];

        int currentIndex = ranked.FindIndex(a => a.UserId == currentUserId);
        LeaderboardEntryResponse? currentUser = currentIndex >= 0 ? ToEntry(ranked[currentIndex], currentIndex) : null;

        return new LeaderboardResponse(entries, currentUser);
    }
}
