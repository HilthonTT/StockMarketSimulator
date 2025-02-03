using StockMarketSimulator.Api.Infrastructure.Messaging;

namespace StockMarketSimulator.Api.Modules.Users.Application.Login;

public sealed record LoginUserCommand(string Email, string Password) : ICommand<TokenResponse>;
