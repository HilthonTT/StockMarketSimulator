using Microsoft.Extensions.Primitives;
using OpenTelemetry;
using OpenTelemetry.Context.Propagation;
using System.Diagnostics;

namespace Web.Api.Middleware;

internal sealed class RequestContextLoggingMiddleware : IMiddleware
{
    private const string CorrelationIdHeaderName = "X-Correlation-Id";
    private static readonly ActivitySource ActivitySource = new("Web.Api.RequestContext");
    private static readonly TextMapPropagator Propagator = Propagators.DefaultTextMapPropagator;

    public Task InvokeAsync(HttpContext context, RequestDelegate next)
    {
        PropagationContext parentContext = Propagator.Extract(default, context, ExtractHeaderValue);
        Baggage.Current = parentContext.Baggage;

        using Activity? activity = ActivitySource.StartActivity("Incoming Request", ActivityKind.Server);

        if (activity is not null)
        {
            string correlationId = GetCorrelationId(context);
            activity.SetTag("correlation_id", correlationId);
            activity.SetTag("http.method", context.Request.Method);
            activity.SetTag("http.url", context.Request.Path);
        }

        return next(context);
    }

    private static string GetCorrelationId(HttpContext context)
    {
        context.Request.Headers.TryGetValue(CorrelationIdHeaderName, out StringValues correlationId);

        return correlationId.FirstOrDefault() ?? context.TraceIdentifier;
    }

    private static IEnumerable<string> ExtractHeaderValue(HttpContext context, string key)
    {
        if (context.Request.Headers.TryGetValue(key, out var values))
        {
            return values;
        }

        return [];
    }
}
