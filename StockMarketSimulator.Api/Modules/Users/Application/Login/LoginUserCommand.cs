using StockMarketSimulator.Api.Infrastructure.Messaging;

namespace StockMarketSimulator.Api.Modules.Users.Application.Login;

internal sealed record LoginUserCommand(string Email, string Password) : ICommand<TokenResponse>;
