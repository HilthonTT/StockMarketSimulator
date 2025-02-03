namespace StockMarketSimulator.Api.Modules.Users.Application.Login;

public sealed record TokenResponse(string AccessToken, string RefreshToken);
