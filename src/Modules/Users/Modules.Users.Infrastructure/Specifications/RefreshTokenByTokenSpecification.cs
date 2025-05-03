using Infrastructure.Database.Specifications;
using Modules.Users.Domain.Entities;

namespace Modules.Users.Infrastructure.Specifications;

internal class RefreshTokenByTokenSpecification : Specification<RefreshToken>
{
    public RefreshTokenByTokenSpecification(string token)
        : base(refreshToken => refreshToken.Token == token)
    {
        AddInclude(refreshToken => refreshToken.User);
    }
}
