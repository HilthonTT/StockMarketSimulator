using System.Net;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Web.Api.RateLimiting;

namespace Application.UnitTests.Web;

public sealed class GlobalLimiterFactoryTests
{
    [Fact]
    public void GetPartitionKey_Should_SeparateUsers()
    {
        string first = GlobalLimiterFactory.GetPartitionKey(ContextFor(Guid.CreateVersion7()));
        string second = GlobalLimiterFactory.GetPartitionKey(ContextFor(Guid.CreateVersion7()));

        Assert.NotEqual(first, second);
    }

    [Fact]
    public void GetPartitionKey_Should_UseTheClientIp_ForAnonymousCallers()
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("203.0.113.7");

        Assert.EndsWith(":ip:203.0.113.7", GlobalLimiterFactory.GetPartitionKey(context));
    }

    private static DefaultHttpContext ContextFor(Guid userId) => new()
    {
        User = new ClaimsPrincipal(new ClaimsIdentity(
            [new Claim(ClaimTypes.NameIdentifier, userId.ToString())],
            authenticationType: "Bearer"))
    };
}
