using System.Security.Claims;
using Web.Api.Features;

namespace Application.UnitTests.Web;

public sealed class UserTargetingContextTests
{
    [Fact]
    public void GetUserGroups_Should_UseRoleAndGroupClaims()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(
            [
                new Claim(ClaimTypes.Role, "Admin"),
                new Claim(UserTargetingContext.GroupsClaimType, "beta-testers"),
                new Claim(UserTargetingContext.GroupsClaimType, "BETA-TESTERS"),
                new Claim("email", "someone@test.com"),
            ],
            authenticationType: "Bearer"));

        string[] groups = UserTargetingContext.GetUserGroups(principal);

        Assert.Equal(["Admin", "beta-testers"], groups);
    }

    [Fact]
    public void GetUserGroups_Should_BeEmpty_ForAnonymousUsers()
    {
        var anonymous = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Role, "Admin")]));

        Assert.Empty(UserTargetingContext.GetUserGroups(anonymous));
    }
}
