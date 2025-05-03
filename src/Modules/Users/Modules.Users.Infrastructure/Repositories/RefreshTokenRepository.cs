using Infrastructure.Database.Specifications;
using Microsoft.EntityFrameworkCore;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Repositories;
using Modules.Users.Infrastructure.Database;
using Modules.Users.Infrastructure.Specifications;
using SharedKernel;

namespace Modules.Users.Infrastructure.Repositories;

internal sealed class RefreshTokenRepository(UsersDbContext context) : IRefreshTokenRepository
{
    public Task<int> BatchDeleteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return ApplySpecification(new RefreshTokenByUserIdSpecification(userId))
            .ExecuteDeleteAsync(cancellationToken);
    }

    public async Task<Option<RefreshToken>> GetByTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        RefreshToken? refreshToken = await ApplySpecification(new RefreshTokenByTokenSpecification(token))
            .FirstOrDefaultAsync(cancellationToken);

        return Option<RefreshToken>.Some(refreshToken);
    }

    public void Insert(RefreshToken refreshToken)
    {
        context.RefreshTokens.Add(refreshToken);
    }

    public void Remove(RefreshToken refreshToken)
    {
        context.RefreshTokens.Remove(refreshToken);
    }

    private IQueryable<RefreshToken> ApplySpecification(Specification<RefreshToken> specification)
    {
        return SpecificationEvaluator.GetQuery(context.RefreshTokens, specification);
    }
}
