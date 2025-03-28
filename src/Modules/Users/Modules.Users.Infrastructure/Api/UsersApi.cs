using Microsoft.EntityFrameworkCore;
using Modules.Users.Api;
using Modules.Users.Infrastructure.Database;

namespace Modules.Users.Infrastructure.Api;

internal sealed class UsersApi(UsersDbContext context) : IUsersApi
{
    public async Task<UserApiResponse?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        UserApiResponse? user = await context.Users
            .AsNoTracking()
            .Where(u => u.Email.Value == email)
            .Select(u => new UserApiResponse(u.Id, u.Email.Value))
            .FirstOrDefaultAsync(cancellationToken);

        return user;
    }

    public async Task<UserApiResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        UserApiResponse? user = await context.Users
            .AsNoTracking()
            .Where(u => u.Id == id)
            .Select(u => new UserApiResponse(u.Id, u.Email.Value))
            .FirstOrDefaultAsync(cancellationToken);

        return user;
    }
}
