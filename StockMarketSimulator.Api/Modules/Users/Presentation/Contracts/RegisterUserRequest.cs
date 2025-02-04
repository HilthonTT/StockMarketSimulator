namespace StockMarketSimulator.Api.Modules.Users.Presentation.Contracts;

internal sealed record RegisterUserRequest(string Email, string Username, string Password);
