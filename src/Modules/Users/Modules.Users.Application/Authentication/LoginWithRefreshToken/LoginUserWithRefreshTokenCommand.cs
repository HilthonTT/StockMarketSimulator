using Application.Abstractions.Messaging;
using Modules.Users.Contracts.Users;

namespace Modules.Users.Application.Authentication.LoginWithRefreshToken;

public sealed record LoginUserWithRefreshTokenCommand(string RefreshToken) : ICommand<TokenResponse>;
