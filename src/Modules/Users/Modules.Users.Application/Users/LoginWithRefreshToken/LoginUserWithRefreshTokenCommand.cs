using Application.Abstractions.Messaging;
using Modules.Users.Contracts.Users;

namespace Modules.Users.Application.Users.LoginWithRefreshToken;

public sealed record LoginUserWithRefreshTokenCommand(string RefreshToken) : ICommand<TokenResponse>;
