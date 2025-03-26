using Microsoft.EntityFrameworkCore;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Repositories;
using Modules.Users.Domain.ValueObjects;
using Modules.Users.Infrastructure.Database;

namespace Modules.Users.Infrastructure.Repositories;

internal sealed class UserRepository(UsersDbContext context) : IUserRepository
{
    public Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default)
    {
        return context.Users.AnyAsync(u => u.Email.Value == email.Value, cancellationToken);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Users.FirstOrDefaultAsync(u => u.Id == id, cancellationToken);
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
