using Modules.Users.Domain.Entities;
using Modules.Users.Domain.ValueObjects;

namespace Modules.Users.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<bool> EmailExistsAsync(Email email, CancellationToken cancellationToken = default);

    void Insert(User user);

    void Remove(User user);
}
