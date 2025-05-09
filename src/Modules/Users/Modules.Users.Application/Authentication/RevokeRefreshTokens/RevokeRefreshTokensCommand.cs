using Application.Abstractions.Messaging;

namespace Modules.Users.Application.Authentication.RevokeRefreshTokens;

public sealed record RevokeRefreshTokensCommand(Guid UserId) : ICommand;
