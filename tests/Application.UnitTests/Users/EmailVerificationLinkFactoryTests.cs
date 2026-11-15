using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using Modules.Users.Infrastructure.Authentication;
using Modules.Users.Infrastructure.Authentication.Options;
using NSubstitute;

namespace Application.UnitTests.Users;

public sealed class EmailVerificationLinkFactoryTests
{
    [Fact]
    public void Create_Should_UseTheConfiguredPublicUrl_NotTheRequestHost()
    {
        // No HttpContext is involved at all, so a forged Host header can't influence the link.
        var factory = new EmailVerificationLinkFactory(
            Options.Create(new AppOptions { PublicApiUrl = "https://api.example.com/" }),
            Substitute.For<LinkGenerator>());

        Guid tokenId = Guid.CreateVersion7();

        string link = factory.Create(tokenId);

        Assert.Equal($"https://api.example.com/api/v1/authentication/verify-email?token={tokenId}", link);
    }

    [Theory]
    [InlineData("http://localhost:5000", true)]
    [InlineData("https://api.example.com", true)]
    [InlineData("", false)]
    [InlineData("localhost:5000", false)]
    [InlineData("ftp://example.com", false)]
    public void AppOptionsValidator_Should_RequireAnAbsoluteHttpUrl(string url, bool valid)
    {
        bool result = new AppOptionsValidator().Validate(new AppOptions { PublicApiUrl = url }).IsValid;

        Assert.Equal(valid, result);
    }
}
