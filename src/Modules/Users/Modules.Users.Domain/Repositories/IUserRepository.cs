using Modules.Users.Domain.Entities;

namespace Modules.Users.Domain.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Insert(User user);

    void Remove(User user);
}
