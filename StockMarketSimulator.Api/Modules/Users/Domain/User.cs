namespace StockMarketSimulator.Api.Modules.Users.Domain;

public sealed class User
{
    public required Guid Id { get; set; }

    public required string Email { get; set; }

    public required string Username { get; set; }

    public required string PasswordHash { get; set; }

    public bool EmailVerified { get; set; }
}
