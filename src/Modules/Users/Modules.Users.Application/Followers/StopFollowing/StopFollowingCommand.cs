using Application.Abstractions.Messaging;

namespace Modules.Users.Application.Followers.StopFollowing;

public sealed record StopFollowingCommand(Guid UserId, Guid FollowedId) : ICommand;
