namespace StockMarketSimulator.Api.Modules.Users.Contracts;

internal sealed record ChangePasswordRequest(string CurrentPassword, string NewPassword);
