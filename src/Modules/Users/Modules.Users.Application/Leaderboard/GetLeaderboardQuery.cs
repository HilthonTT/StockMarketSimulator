using Application.Abstractions.Messaging;
using Modules.Users.Contracts.Leaderboard;

namespace Modules.Users.Application.Leaderboard;

public sealed record GetLeaderboardQuery(int Limit) : IQuery<LeaderboardResponse>;
