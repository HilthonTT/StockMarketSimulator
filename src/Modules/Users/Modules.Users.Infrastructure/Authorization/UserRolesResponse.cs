using Modules.Users.Domain.Entities;

namespace Modules.Users.Infrastructure.Authorization;

public sealed class UserRolesResponse
{
    public Guid Id { get; init; }

    public List<Role> Roles { get; init; } = [];
}
