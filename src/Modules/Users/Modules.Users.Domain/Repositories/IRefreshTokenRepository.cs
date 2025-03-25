using Modules.Users.Domain.Entities;

namespace Modules.Users.Domain.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    void Insert(RefreshToken refreshToken);
}
