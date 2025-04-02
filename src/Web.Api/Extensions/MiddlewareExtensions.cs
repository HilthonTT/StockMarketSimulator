using Web.Api.Middleware;

namespace Web.Api.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseUserContextEnrichment(this IApplicationBuilder app)
    {
        app.UseMiddleware<UserContextEnrichementMiddleware>();

        return app;
    }
}
