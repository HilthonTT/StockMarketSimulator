using Microsoft.EntityFrameworkCore;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Repositories;
using Modules.Users.Domain.ValueObjects;
using Modules.Users.Infrastructure.Database;
using SharedKernel;

namespace Modules.Users.Infrastructure.Repositories;

internal sealed class UserRepository(UsersDbContext context) : IUserRepository
{
    public Task<bool> EmailNotUniqueAsync(Email email, CancellationToken cancellationToken = default)
    {
        return context.Users.AnyAsync(u => u.Email == email, cancellationToken);
    }

    public async Task<Option<User>> GetByEmailAsync(Email email, CancellationToken cancellationToken = default)
    {
        User? user = await context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Email == email, cancellationToken);

        return Option<User>.Some(user);
    }

    public async Task<Option<User>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        User? user = await context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == id, cancellationToken);

        return Option<User>.Some(user);
    }

    public void Insert(User user)
    {
        context.Users.Add(user);
    }

    public void Remove(User user)
    {
        context.Users.Remove(user);
    }
}
