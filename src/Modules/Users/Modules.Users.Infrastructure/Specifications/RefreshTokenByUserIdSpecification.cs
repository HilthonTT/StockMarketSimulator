using Infrastructure.Database.Specifications;
using Modules.Users.Domain.Entities;

namespace Modules.Users.Infrastructure.Specifications;

internal sealed class RefreshTokenByUserIdSpecification : Specification<RefreshToken>
{
    public RefreshTokenByUserIdSpecification(Guid userId) 
        : base(refreshToken => refreshToken.UserId == userId)
    {
        AddInclude(refreshToken => refreshToken.User);
    }
}
