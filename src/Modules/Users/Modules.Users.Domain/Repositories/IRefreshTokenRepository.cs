using Modules.Users.Domain.Entities;

namespace Modules.Users.Domain.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default);

    Task<int> BatchDeleteAsync(Guid userId, CancellationToken cancellationToken = default);

    void Insert(RefreshToken refreshToken);

    void Remove(RefreshToken refreshToken);
}
