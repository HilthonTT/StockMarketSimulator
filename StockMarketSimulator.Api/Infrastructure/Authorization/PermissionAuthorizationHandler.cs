using Microsoft.AspNetCore.Authorization;
using StockMarketSimulator.Api.Modules.Users.Infrastructure;

namespace StockMarketSimulator.Api.Infrastructure.Authorization;

internal sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    private readonly IServiceScopeFactory _serviceScopeFactory;

    public PermissionAuthorizationHandler(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    protected async override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, 
        PermissionRequirement requirement)
    {
        // TODO: You definitely want to reject unauthenticated users here.
        if (context.User is { Identity.IsAuthenticated: true })
        {
            // TODO: Remove this call when you implement the PermissionProvider.GetForUserIdAsync
            context.Succeed(requirement);

            return;
        }

        using IServiceScope scope = _serviceScopeFactory.CreateScope();

        IPermissionProvider permissionProvider = scope.ServiceProvider.GetRequiredService<IPermissionProvider>();

        Guid userId = context.User.GetUserId();

        HashSet<string> permissions = await permissionProvider.GetForUserIdAsync(userId);

        if (permissions.Contains(requirement.Permission))
        {
            context.Succeed(requirement);
        }
    }
}
