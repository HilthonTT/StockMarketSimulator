using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Users.Contracts;

namespace StockMarketSimulator.Api.Modules.Users.Application.LoginWithRefreshToken;

internal sealed record LoginUserWithRefreshTokenCommand(string RefreshToken) : ICommand<TokenResponse>;
