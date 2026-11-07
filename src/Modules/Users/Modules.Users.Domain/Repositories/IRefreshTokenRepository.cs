using Modules.Users.Domain.Entities;
using SharedKernel;

namespace Modules.Users.Domain.Repositories;

public interface IRefreshTokenRepository
{
    Task<Option<RefreshToken>> GetByTokenHashAsync(string tokenHash, CancellationToken cancellationToken = default);

    Task<int> BatchDeleteAsync(Guid userId, CancellationToken cancellationToken = default);

    void Insert(RefreshToken refreshToken);

    void Remove(RefreshToken refreshToken);
}
