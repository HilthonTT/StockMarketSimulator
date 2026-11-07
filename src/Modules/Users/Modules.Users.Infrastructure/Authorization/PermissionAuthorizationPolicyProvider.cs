using System.Collections.Concurrent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Modules.Users.Infrastructure.Authorization;

/// <summary>
/// Builds a <see cref="PermissionRequirement"/> policy on demand for any permission name.
/// </summary>
/// <remarks>
/// Policies are cached in a thread-safe dictionary instead of being added to the shared
/// <see cref="AuthorizationOptions"/>, which is not safe to mutate concurrently.
/// </remarks>
internal sealed class PermissionAuthorizationPolicyProvider(IOptions<AuthorizationOptions> options)
    : DefaultAuthorizationPolicyProvider(options)
{
    private static readonly ConcurrentDictionary<string, AuthorizationPolicy> Policies = new();

    public override async Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        AuthorizationPolicy? policy = await base.GetPolicyAsync(policyName);

        return policy ?? Policies.GetOrAdd(
            policyName,
            static name => new AuthorizationPolicyBuilder()
                .AddRequirements(new PermissionRequirement(name))
                .Build());
    }
}
