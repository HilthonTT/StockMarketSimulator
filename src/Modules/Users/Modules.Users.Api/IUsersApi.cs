namespace Modules.Users.Api;

public interface IUsersApi
{
    Task<UserApiResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<UserApiResponse?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
