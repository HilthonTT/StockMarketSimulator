using Microsoft.EntityFrameworkCore;
using Modules.Users.Api.Api;
using Modules.Users.Api.Responses;
using Modules.Users.Infrastructure.Database;
using SharedKernel;

namespace Modules.Users.Infrastructure.Api;

internal sealed class UsersApi(UsersDbContext context) : IUsersApi
{
    private static readonly Func<UsersDbContext, Guid, Task<UserApiResponse?>> GetUserById =
        EF.CompileAsyncQuery((UsersDbContext dbContext, Guid id) =>
            dbContext.Users
                .Where(u => u.Id == id)
                .Select(u => new UserApiResponse(u.Id, u.Email.Value))
                .FirstOrDefault());

    private static readonly Func<UsersDbContext, string, Task<UserApiResponse?>> GetUserByEmail =
        EF.CompileAsyncQuery((UsersDbContext dbContext, string email) =>
            dbContext.Users
                .Where(u => u.Email.Value == email)
                .Select(u => new UserApiResponse(u.Id, u.Email.Value))
                .FirstOrDefault());

    public async Task<Option<UserApiResponse>> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        UserApiResponse? user = await GetUserByEmail(context, email);

        return Option<UserApiResponse>.Some(user);
    }

    public async Task<Option<UserApiResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        UserApiResponse? user = await GetUserById(context, id);

        return Option<UserApiResponse>.Some(user);
    }
}
