using Application.Abstractions.Caching;
using Infrastructure.Authentication;
using Microsoft.Extensions.Primitives;

namespace Web.Api.Idempotency;

/// <summary>
/// Makes an endpoint safe to retry with the same <c>Idempotence-Key</c> header.
/// </summary>
/// <remarks>
/// <list type="bullet">
/// <item>Keys are scoped to the caller, the HTTP method and the route, so a key reused on another endpoint
/// or by another user never replays someone else's response.</item>
/// <item>Before running the handler the key is claimed atomically. A duplicate that arrives while the first
/// request is still running gets <c>409 Conflict</c> instead of executing a second time.</item>
/// <item>Successful (2xx) responses with a body are stored and replayed. On failure the claim is released so
/// the client can retry.</item>
/// </list>
/// </remarks>
internal sealed class IdempotencyFilter : IEndpointFilter
{
    internal const string HeaderName = "Idempotence-Key";

    private static readonly TimeSpan ResponseLifetime = TimeSpan.FromMinutes(60);

    // How long an in-flight claim blocks duplicates. Bounded so a crashed request doesn't lock the key for
    // the whole response lifetime.
    private static readonly TimeSpan ClaimLifetime = TimeSpan.FromMinutes(2);

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        HttpContext httpContext = context.HttpContext;

        if (!TryGetIdempotenceKey(httpContext.Request.Headers, out Guid idempotenceKey))
        {
            return Results.BadRequest($"Invalid or missing {HeaderName} header");
        }

        ICacheService cache = httpContext.RequestServices.GetRequiredService<ICacheService>();
        CancellationToken cancellationToken = httpContext.RequestAborted;

        string cacheKey = CreateCacheKey(httpContext, idempotenceKey);

        IdempotentResponse? cached = await cache.GetAsync<IdempotentResponse>(cacheKey, cancellationToken);
        if (cached is not null)
        {
            return new IdempotentResult(cached.StatusCode, cached.Value);
        }

        if (!await cache.TryClaimAsync(cacheKey, ClaimLifetime, cancellationToken))
        {
            // Either still in flight, or it just finished between the two reads.
            cached = await cache.GetAsync<IdempotentResponse>(cacheKey, cancellationToken);

            return cached is not null
                ? new IdempotentResult(cached.StatusCode, cached.Value)
                : Results.Conflict($"A request with this {HeaderName} is already being processed.");
        }

        object? result;
        try
        {
            result = await next(context);
        }
        catch
        {
            await cache.ReleaseClaimAsync(cacheKey, CancellationToken.None);
            throw;
        }

        if (result is IStatusCodeHttpResult { StatusCode: >= 200 and < 300 } statusCodeResult
            and IValueHttpResult valueHttpResult)
        {
            int statusCode = statusCodeResult.StatusCode ?? StatusCodes.Status200OK;

            await cache.SetAsync(
                cacheKey,
                new IdempotentResponse(statusCode, valueHttpResult.Value),
                ResponseLifetime,
                CancellationToken.None);
        }
        else
        {
            // Nothing to replay (error or empty body): let the client retry with the same key.
            await cache.ReleaseClaimAsync(cacheKey, CancellationToken.None);
        }

        return result;
    }

    internal static string CreateCacheKey(HttpContext httpContext, Guid idempotenceKey)
    {
        string caller = httpContext.User.TryGetUserId(out Guid userId) ? userId.ToString() : "anonymous";

        string route = (httpContext.GetEndpoint() as RouteEndpoint)?.RoutePattern.RawText
            ?? httpContext.Request.Path.Value
            ?? string.Empty;

        return $"idempotency:{caller}:{httpContext.Request.Method}:{route}:{idempotenceKey}";
    }

    private static bool TryGetIdempotenceKey(IHeaderDictionary headers, out Guid idempotenceKey)
    {
        if (headers.TryGetValue(HeaderName, out StringValues idempotenceKeyValue) &&
            Guid.TryParse(idempotenceKeyValue, out idempotenceKey))
        {
            return true;
        }

        idempotenceKey = Guid.Empty;

        return false;
    }
}
