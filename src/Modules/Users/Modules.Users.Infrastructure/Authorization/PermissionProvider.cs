using Application.Abstractions.Caching;
using Microsoft.EntityFrameworkCore;
using Modules.Users.Domain.Entities;
using Modules.Users.Infrastructure.Database;

namespace Modules.Users.Infrastructure.Authorization;

internal sealed class PermissionProvider(UsersDbContext context, ICacheService cacheService)
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

    public async Task<UserRolesResponse?> GetRolesForUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        string cacheKey = $"auth:roles-{userId}";

        UserRolesResponse? cachedRoles = 
            await cacheService.GetAsync<UserRolesResponse>(cacheKey, cancellationToken);

        if (cachedRoles is not null)
        {
            return cachedRoles;
        }

        UserRolesResponse? roles = await context.Users
            .AsNoTracking()
            .Where(user => user.Id == userId)
            .Include(u => u.Roles)
            .Select(user => new UserRolesResponse
            {
                Id = user.Id,
                Roles = user.Roles.ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (roles is not null)
        {
            await cacheService.SetAsync(cacheKey, roles, cancellationToken: cancellationToken);
        }

        return roles;
    }
}
