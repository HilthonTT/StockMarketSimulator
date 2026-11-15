namespace Modules.Users.Contracts.Leaderboard;

public sealed record LeaderboardEntryResponse(
    int Rank,
    Guid UserId,
    string Username,
    decimal AccountValue,
    decimal Cash,
    decimal HoldingsValue,
    decimal ReturnPercent,
    int Positions);

public sealed record LeaderboardResponse(
    IReadOnlyList<LeaderboardEntryResponse> Entries,
    LeaderboardEntryResponse? CurrentUser);
