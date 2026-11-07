using Modules.Users.Domain.Entities;

namespace Domain.UnitTests.Users;

public sealed class RefreshTokenTests
{
    [Fact]
    public void Refresh_ShouldRotateToken_ButKeepAbsoluteExpiry()
    {
        var token = RefreshToken.Create("initial-token", Guid.CreateVersion7());
        DateTime originalExpiry = token.ExpiresOnUtc;

        token.Refresh("rotated-token");

        Assert.Equal("rotated-token", token.TokenHash);
        Assert.Equal(originalExpiry, token.ExpiresOnUtc);
    }
}
