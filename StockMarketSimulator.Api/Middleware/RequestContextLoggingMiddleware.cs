using Microsoft.Extensions.Primitives;
using Serilog.Context;

namespace StockMarketSimulator.Api.Middleware;

public sealed class RequestContextLoggingMiddleware
{
    private const string CorrelationIdHeaderName = "Correlation-Id";

    private readonly RequestDelegate _next;

    public RequestContextLoggingMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public Task Invoke(HttpContext context)
    {
        using (LogContext.PushProperty("CorrelationId", GetCorrelationId(context)))
        {
            return _next(context);
        }
    }

    private static string GetCorrelationId(HttpContext context)
    {
        context.Request.Headers.TryGetValue(
            CorrelationIdHeaderName,
            out StringValues correlationId);

        return correlationId.FirstOrDefault() ?? context.TraceIdentifier;
    }
}
