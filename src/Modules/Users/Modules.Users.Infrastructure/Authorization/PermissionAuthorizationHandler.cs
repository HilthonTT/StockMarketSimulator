using Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using SharedKernel;

namespace Modules.Users.Infrastructure.Authorization;

internal sealed class PermissionAuthorizationHandler(IServiceScopeFactory serviceScopeFactory)
    : AuthorizationHandler<PermissionRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        try
        {
            using IServiceScope scope = serviceScopeFactory.CreateScope();

            PermissionProvider permissionProvider = scope.ServiceProvider.GetRequiredService<PermissionProvider>();

            if (!context.User.TryGetUserId(out Guid userId))
            {
                context.Fail();
                return;
            }

            HashSet<string> permissions = await permissionProvider.GetForUserIdAsync(userId);

            if (permissions.Contains(requirement.Permission))
            {
                context.Succeed(requirement);
            }
        }
        catch (UnauthorizedException)
        {
            context.Fail();
        }
    }
}
