using StockMarketSimulator.Api.Middleware;

namespace StockMarketSimulator.Api.Extensions;

public static class MiddlewareExtensions
{
    public static IApplicationBuilder UseRequestContextLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestContextLoggingMiddleware>();

        return app;
    }

    public static IApplicationBuilder UseUserContextEnrichment(this IApplicationBuilder app)
    {
        app.UseMiddleware<UserContextEnrichementMiddleware>();

        return app;
    }
}
