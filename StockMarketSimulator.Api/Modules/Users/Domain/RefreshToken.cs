namespace StockMarketSimulator.Api.Modules.Users.Domain;

internal sealed class RefreshToken
{
    public Guid Id { get; set; }

    public required string Token { get; set; }

    public required Guid UserId { get; set; }

    public required DateTime ExpiresOnUtc { get; set; }
}
