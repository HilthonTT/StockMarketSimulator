using StockMarketSimulator.Api.Infrastructure.Events;

namespace StockMarketSimulator.Api.Modules.Users.Application.Register;

public sealed record RegisterUserCommand(string Email, string Username, string Password) : IIntegrationEvent;
