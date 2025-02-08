using StockMarketSimulator.Api.Infrastructure.Messaging;

namespace StockMarketSimulator.Api.Modules.Users.Application.Register;

internal sealed record RegisterUserCommand(string Email, string Username, string Password, string ConfirmPassword) : ICommand;
