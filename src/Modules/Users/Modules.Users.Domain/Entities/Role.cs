using SharedKernel;

namespace Modules.Users.Domain.Entities;

public sealed class Role(int id, string name) : Enumeration<Role>(id, name)
{
    public static readonly Role Registered = new(1, "Registered");

    public static readonly Role Admin = new(2, "Admin");

    public List<Permission> Permissions { get; set; } = [];

    public List<User> Users { get; set; } = [];
}
