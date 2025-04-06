using Modules.Users.Domain.Entities;
using Modules.Users.Domain.ValueObjects;
using SharedKernel;

namespace Modules.Users.Domain.Repositories;

public interface IUserRepository
{
    Task<Option<User>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Option<User>> GetByEmailAsync(Email email, CancellationToken cancellationToken = default);

    Task<bool> EmailNotUniqueAsync(Email email, CancellationToken cancellationToken = default);

    void Insert(User user);

    void Remove(User user);
}
