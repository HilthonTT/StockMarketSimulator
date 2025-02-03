using StockMarketSimulator.Api.Infrastructure.Messaging;

namespace StockMarketSimulator.Api.Modules.Users.Application.Register;

public sealed record RegisterUserCommand(string Email, string Username, string Password) : ICommand;
