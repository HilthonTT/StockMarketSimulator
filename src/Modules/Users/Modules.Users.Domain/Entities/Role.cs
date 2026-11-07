using SharedKernel;

namespace Modules.Users.Domain.Entities;

public sealed class Role(int id, string name) : Enumeration<Role>(id, name)
{
    public static readonly Role Registered = new(1, "Registered");

    public static readonly Role Admin = new(2, "Admin");

    public List<Permission> Permissions { get; set; } = [];

    /// <summary>
    /// Creates a new, untracked instance with the same identity, for attaching to a DbContext.
    /// </summary>
    /// <remarks>
    /// The static <see cref="Registered"/>/<see cref="Admin"/> instances are shared by the whole
    /// process. Attaching them to a DbContext lets EF Core fix up navigations on shared mutable
    /// objects across concurrent requests, so persistence code must use a copy instead.
    /// </remarks>
    public Role CreateReference() => new(Id, Name);
}
