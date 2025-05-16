using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace Yarp.Proxy;

public static class DependencyInjection
{
    public static IServiceCollection ConfigureRateLimiter(this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddTokenBucketLimiter("token-bucket", limiterOptions =>
            {
                limiterOptions.TokenLimit = 100;   // Maximum tokens (burst capacity)
                limiterOptions.TokensPerPeriod = 50; // Refill rate (tokens per period)
                limiterOptions.ReplenishmentPeriod = TimeSpan.FromSeconds(1); // Refilling interval
            });

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out TimeSpan retryAfter))
                {
                    await context.HttpContext.Response.WriteAsync(
                        $"Too many requests. Please try again after {retryAfter.TotalSeconds} second(s).",
                        cancellationToken);
                }
                else
                {
                    await context.HttpContext.Response.WriteAsync(
                       "Too many requests. Please try again later.",
                       cancellationToken);
                }
            };
        });

        return services;
    }
}
