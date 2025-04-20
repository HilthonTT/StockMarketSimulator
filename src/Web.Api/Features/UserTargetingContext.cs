using Microsoft.FeatureManagement.FeatureFilters;

namespace Web.Api.Features;

internal sealed class UserTargetingContext : ITargetingContextAccessor
{
    private const string CacheKey = "UserTargetingContext.TargetingContext";

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
            Groups = GetUserGroups(httpContext),
        };

        return new ValueTask<TargetingContext>(targetingContext);
    }

    private static string GetUserId(HttpContext? httpContext)
    {
        // For demo purposes, this might come for JWT claims.

        return httpContext?.Request.Headers["X-User-Id"].FirstOrDefault() ?? string.Empty;
    }

    private static string[] GetUserGroups(HttpContext? httpContext)
    {
        // For demo purposes, these might come from a database/claims
        string[] userGroups = httpContext?.Request
            .Headers["X-User-Groups"]
            .FirstOrDefault()?
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries) ?? [];

        return userGroups;
    }
}
