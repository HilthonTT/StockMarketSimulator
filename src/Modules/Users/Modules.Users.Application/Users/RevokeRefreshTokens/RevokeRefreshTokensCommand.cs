using Application.Abstractions.Messaging;

namespace Modules.Users.Application.Users.RevokeRefreshTokens;

public sealed record RevokeRefreshTokensCommand(Guid UserId) : ICommand;
