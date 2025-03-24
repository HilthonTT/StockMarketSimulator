using SharedKernel;

namespace StockMarketSimulator.Api.Modules.Roles.Domain;

internal sealed class UserRole : IEntity
{
    private UserRole()
    {
    }

    private UserRole(Guid userId, int roleId)
    {
        Ensure.NotNullOrEmpty(userId, nameof(userId));
        Ensure.GreaterThanOrEqualToZero(roleId, nameof(roleId));

        UserId = userId;
        RoleId = roleId;
    }

    public Guid UserId { get; set; }

    public int RoleId { get; set; } 

    internal static UserRole CreateMember(Guid userId)
    {
        var userRole = new UserRole(userId, Role.MemberId);

        return userRole;
    }

    internal static UserRole Create(Guid userId, int roleId)
    {
        var userRole = new UserRole(userId, Role.MemberId);

        return userRole;
    }
}
