using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Users.Contracts;

namespace StockMarketSimulator.Api.Modules.Users.Application.Login;

internal sealed record LoginUserCommand(string Email, string Password) : ICommand<TokenResponse>;
