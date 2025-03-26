using Microsoft.EntityFrameworkCore;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Repositories;
using Modules.Users.Infrastructure.Database;

namespace Modules.Users.Infrastructure.Repositories;

internal sealed class RefreshTokenRepository(UsersDbContext context) : IRefreshTokenRepository
{
    public Task<RefreshToken?> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        return context.RefreshTokens
            .Include(r => r.User)
            .FirstOrDefaultAsync(r => r.Token == token, cancellationToken);
    }

    public void Insert(RefreshToken refreshToken)
    {
        context.RefreshTokens.Add(refreshToken);
    }

    public void Remove(RefreshToken refreshToken)
    {
        context.RefreshTokens.Remove(refreshToken);
    }
}
