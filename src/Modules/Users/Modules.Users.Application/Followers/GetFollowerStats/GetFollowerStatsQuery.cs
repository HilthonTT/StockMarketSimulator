using Application.Abstractions.Messaging;
using Modules.Users.Contracts.Followers;

namespace Modules.Users.Application.Followers.GetFollowerStats;

public sealed record GetFollowerStatsQuery(Guid UserId) : IQuery<FollowerStatsResponse>;
