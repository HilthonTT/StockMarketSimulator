using Infrastructure.Database.Specifications;
using Modules.Users.Domain.Entities;

namespace Modules.Users.Infrastructure.Specifications;

internal sealed class RefreshTokenByTokenHashSpecification : Specification<RefreshToken>
{
    public RefreshTokenByTokenHashSpecification(string tokenHash)
        : base(refreshToken => refreshToken.TokenHash == tokenHash)
    {
        AddInclude(refreshToken => refreshToken.User);
    }
}
