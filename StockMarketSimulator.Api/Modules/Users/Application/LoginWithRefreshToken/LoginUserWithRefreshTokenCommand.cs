using StockMarketSimulator.Api.Infrastructure.Messaging;

namespace StockMarketSimulator.Api.Modules.Users.Application.LoginWithRefreshToken;

internal sealed record LoginUserWithRefreshTokenCommand(string RefreshToken) : ICommand<TokenResponse>;
