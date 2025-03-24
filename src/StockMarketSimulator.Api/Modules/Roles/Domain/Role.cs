using SharedKernel;

namespace StockMarketSimulator.Api.Modules.Roles.Domain;

internal sealed class Role : IEntity
{
    public const string Admin = "Admin";
    public const string Member = "Member";
    public const int AdminId = 1;
    public const int MemberId = 2;

    private Role()
    {
    }

    private Role(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public int Id { get; init; }

    public string Name { get; init; } = string.Empty;

    internal static Role Create(int id, string name)
    {
        return new(id, name);
    }
}
