using System.Diagnostics;
using System.Security.Claims;

namespace StockMarketSimulator.Api.Middleware;

internal sealed class UserContextEnrichementMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<UserContextEnrichementMiddleware> _logger;

    public UserContextEnrichementMiddleware(
        RequestDelegate next,
        ILogger<UserContextEnrichementMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        string? userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!string.IsNullOrWhiteSpace(userId))
        {
            Activity.Current?.SetTag("user.id", userId);

            var data = new Dictionary<string, object>
            {
                ["UserId"] = userId,
            };

            using (_logger.BeginScope(data))
            {
                await _next(context);
            }
        }
        else
        {
            await _next(context);
        }
    }
}
