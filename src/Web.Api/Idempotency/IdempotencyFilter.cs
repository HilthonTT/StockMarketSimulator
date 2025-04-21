using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Primitives;

namespace Web.Api.Idempotency;

internal sealed class IdempotencyFilter : IEndpointFilter
{
    private const int DefaultCacheTimeInMinutes = 60;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (!TryGetIdempotenceKey(context.HttpContext.Request.Headers, out Guid idempotenceKey))
        {
            return Results.BadRequest("Invalid or missing Idempotence-Key header");
        }

        IDistributedCache cache = context.HttpContext.RequestServices.GetRequiredService<IDistributedCache>();

        // Check if we already processed this request and return a cached response (if it exists)
        string cacheKey = $"Idempotent_{idempotenceKey}";
        string? cachedResult = await cache.GetStringAsync(cacheKey);
        if (!string.IsNullOrWhiteSpace(cachedResult))
        {
            IdempotentResponse response = JsonSerializer.Deserialize<IdempotentResponse>(cachedResult)!;
            return new IdempotentResult(response.StatusCode, response.Value);
        }

        object? result = await next(context);

        // Execute the request and cache the response for the specified duration
        if (result is IStatusCodeHttpResult { StatusCode: >= 200 and < 300 } statusCodeResult 
            and IValueHttpResult valueHttpResult)
        {
            int statusCode = statusCodeResult.StatusCode ?? StatusCodes.Status200OK;
            IdempotentResponse response = new(statusCode, valueHttpResult.Value);

            await cache.SetStringAsync(
                cacheKey,
                JsonSerializer.Serialize(response),
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(DefaultCacheTimeInMinutes)
                }
            );
        }

        return result;
    }

    private static bool TryGetIdempotenceKey(IHeaderDictionary headers, out Guid idempotenceKey)
    {
        if (headers.TryGetValue("Idempotence-Key", out StringValues idempotenceKeyValue) &&
            Guid.TryParse(idempotenceKeyValue, out idempotenceKey))
        {
            return true;
        }

        idempotenceKey = Guid.Empty;

        return false;
    }
}
