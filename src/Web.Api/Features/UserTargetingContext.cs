using System.Security.Claims;
using Infrastructure.Authentication;
using Microsoft.FeatureManagement.FeatureFilters;

namespace Web.Api.Features;

internal sealed class UserTargetingContext : ITargetingContextAccessor
{
    private const string CacheKey = "UserTargetingContext.TargetingContext";

    /// <summary>Optional claim carrying feature-targeting groups (one claim per group).</summary>
    internal const string GroupsClaimType = "groups";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public UserTargetingContext(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public ValueTask<TargetingContext> GetContextAsync()
    {
        HttpContext? httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            return new ValueTask<TargetingContext>(new TargetingContext
            {
                UserId = string.Empty,
                Groups = []
            });
        }

        if (httpContext.Items.TryGetValue(CacheKey, out object? value))
        {
            return new ValueTask<TargetingContext>((TargetingContext)value!);
        }

        var targetingContext = new TargetingContext
        {
            UserId = GetUserId(httpContext),
            Groups = GetUserGroups(httpContext.User),
        };

        httpContext.Items[CacheKey] = targetingContext;

        return new ValueTask<TargetingContext>(targetingContext);
    }

    private static string GetUserId(HttpContext httpContext)
    {
        return httpContext.User.TryGetUserId(out Guid userId) ?
            userId.ToString() : string.Empty;
    }

    /// <summary>
    /// Targeting groups come from the authenticated principal only: role claims (added by
    /// <c>CustomClaimsTransformation</c>) plus an optional <c>groups</c> claim. Nothing client-supplied
    /// (headers, query string) is trusted, so a caller can't opt themselves into a feature.
    /// </summary>
    internal static string[] GetUserGroups(ClaimsPrincipal user)
    {
        if (user.Identity?.IsAuthenticated != true)
        {
            return [];
        }

        return [.. user.Claims
            .Where(claim => claim.Type is ClaimTypes.Role or GroupsClaimType)
            .Select(claim => claim.Value.Trim())
            .Where(value => value.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)];
    }
}
