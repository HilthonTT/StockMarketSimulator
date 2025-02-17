namespace StockMarketSimulator.Api.Modules.Roles.Domain;

internal sealed class Role
{
    public const string Admin = "Admin";
    public const string Member = "Member";
    public const int AdminId = 1;
    public const int MemberId = 2;

    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;
}
