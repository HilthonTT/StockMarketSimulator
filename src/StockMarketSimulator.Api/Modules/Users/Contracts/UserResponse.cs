namespace StockMarketSimulator.Api.Modules.Users.Contracts;

internal sealed record UserResponse(Guid Id, string Email, string Username);
