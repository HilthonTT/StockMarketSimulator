using System.Diagnostics;
using Microsoft.Extensions.Primitives;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;

namespace Web.Api.Middleware;

internal sealed class RequestContextLoggingMiddleware(ILogger<RequestContextLoggingMiddleware> logger) : IMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-Id";

    // Picked up by the "<ApplicationName>.*" source registration in ServiceDefaults.
    private static readonly ActivitySource ActivitySource = new("Web.Api.RequestContext");
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    public async Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        PropagationContext parentContext = Propagator.Extract(default, context, ExtractHeaderValue);
        Baggage.Current = parentContext.Baggage;

        string correlationId = GetCorrelationId(context);

        // Both the activity and the log scope must live until the downstream pipeline completes.
        using Activity? activity = ActivitySource.StartActivity("Incoming Request", ActivityKind.Server);
        using IDisposable? scope = logger.BeginScope(
            new Dictionary<string, object> { ["CorrelationId"] = correlationId });

        if (activity is not null)
        {
            activity.SetTag("correlation_id", correlationId);
            activity.SetTag("http.method", context.Request.Method);
            activity.SetTag("http.url", context.Request.Path);
        }

        await next(context);
    }

    private static string GetCorrelationId(HttpContext context)
    {
        context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out StringValues correlationId);

        return correlationId.FirstOrDefault() ?? context.TraceIdentifier;
    }

    private static IEnumerable<string> ExtractHeaderValue(HttpContext context, string key)
    {
        return context.Request.Headers.TryGetValue(key, out StringValues values)
            ? values.Where(value => value is not null).Select(value => value!)
            : [];
    }
}
