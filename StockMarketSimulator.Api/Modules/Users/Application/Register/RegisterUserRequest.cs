namespace StockMarketSimulator.Api.Modules.Users.Application.Register;

public sealed record RegisterUserRequest(string Email, string Username, string Password);
