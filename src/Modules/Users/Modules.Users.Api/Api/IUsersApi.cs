using Modules.Users.Api.Responses;
using SharedKernel;

namespace Modules.Users.Api.Api;

public interface IUsersApi
{
    Task<Option<UserApiResponse>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Option<UserApiResponse>> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}
