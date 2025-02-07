namespace StockMarketSimulator.Api.Modules.Users.Contracts;

internal sealed record RegisterUserRequest(string Email, string Username, string Password);
