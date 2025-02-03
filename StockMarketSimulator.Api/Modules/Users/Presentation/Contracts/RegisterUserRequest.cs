namespace StockMarketSimulator.Api.Modules.Users.Presentation.Contracts;

public sealed record RegisterUserRequest(string Email, string Username, string Password);
