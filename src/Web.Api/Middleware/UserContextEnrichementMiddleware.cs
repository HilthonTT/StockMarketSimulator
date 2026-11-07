using System.Diagnostics;
using System.Security.Claims;

namespace Web.Api.Middleware;

internal sealed class UserContextEnrichementMiddleware(ILogger<UserContextEnrichementMiddleware> logger)
    : IMiddleware
{
    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        string? userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);

        if (string.IsNullOrWhiteSpace(userId))
        {
            await next(context);
            return;
        }

        Activity.Current?.SetTag("user.id", userId);

        // The scope must stay open until the rest of the pipeline has completed,
        // so the call to next has to be awaited inside the using block.
        using (logger.BeginScope(new Dictionary<string, object> { ["UserId"] = userId }))
        {
            await next(context);
        }
    }
}
