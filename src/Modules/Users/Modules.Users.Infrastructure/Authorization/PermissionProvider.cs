using Microsoft.EntityFrameworkCore;
using Modules.Users.Domain.Entities;
using Modules.Users.Infrastructure.Database;

namespace Modules.Users.Infrastructure.Authorization;

internal sealed class PermissionProvider(UsersDbContext context)
{
    public async Task<HashSet<string>> GetForUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Role>[] roles = await context.Users
           .Include(x => x.Roles)
           .ThenInclude(x => x.Permissions)
           .Where(x => x.Id == userId)
           .Select(x => x.Roles)
           .ToArrayAsync(cancellationToken);

        return [.. roles
            .SelectMany(x => x)
            .SelectMany(x => x.Permissions)
            .Select(x => x.Name)];
    }
}
