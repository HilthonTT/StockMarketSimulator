namespace Modules.Users.Domain.Entities;

public sealed class RolePermission
{
    public int RoleId { get; init; }

    public int PermissionId { get; init; }
}
