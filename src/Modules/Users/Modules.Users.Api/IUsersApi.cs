using SharedKernel;

namespace Modules.Users.Api;

public interface IUsersApi
{
    Task<Option<UserApiResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Option<UserApiResponse>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
