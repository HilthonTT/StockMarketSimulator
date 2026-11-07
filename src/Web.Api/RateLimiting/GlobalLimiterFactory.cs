using System.Threading.RateLimiting;
using RedisRateLimiting;
using StackExchange.Redis;

namespace Web.Api.RateLimiting;

/// <summary>
/// Builds the limiter behind <c>GlobalLimiter</c>: a Redis token bucket shared by every instance, with an
/// in-process token bucket of the same shape as the fallback.
/// </summary>
internal static class GlobalLimiterFactory
{
    public const int TokenLimit = 100;          // Maximum tokens (burst capacity)
    public const int TokensPerPeriod = 50;      // Refill rate (tokens per period)
    public static readonly TimeSpan ReplenishmentPeriod = TimeSpan.FromSeconds(1);

    public static RateLimiter Create(string partitionKey, IConnectionMultiplexer? connectionMultiplexer, ILogger logger)
    {
        RateLimiter localLimiter = CreateLocal();

        if (connectionMultiplexer is null)
        {
            // Postgres-cache fallback mode: Redis was never reachable, so limit per instance.
            return localLimiter;
        }

        var redisLimiter = new RedisTokenBucketRateLimiter<string>(
            partitionKey,
            new RedisTokenBucketRateLimiterOptions
            {
                ConnectionMultiplexerFactory = () => connectionMultiplexer,
                TokenLimit = TokenLimit,
                TokensPerPeriod = TokensPerPeriod,
                ReplenishmentPeriod = ReplenishmentPeriod,
            });

        return new RedisFallbackRateLimiter(redisLimiter, localLimiter, logger);
    }

    internal static TokenBucketRateLimiter CreateLocal() =>
        new(new TokenBucketRateLimiterOptions
        {
            TokenLimit = TokenLimit,
            TokensPerPeriod = TokensPerPeriod,
            ReplenishmentPeriod = ReplenishmentPeriod,
            QueueLimit = 0,
            AutoReplenishment = true,
        });
}
