using System.Threading.RateLimiting;
using StackExchange.Redis;

namespace Web.Api.RateLimiting;

/// <summary>
/// Wraps the distributed (Redis) token-bucket limiter and falls back to an in-process token bucket when
/// Redis can't be reached, so rate-limited endpoints keep working (limited per instance instead of
/// cluster-wide) rather than failing with 500s.
/// </summary>
internal sealed class RedisFallbackRateLimiter(
    RateLimiter redisLimiter,
    RateLimiter localLimiter,
    ILogger logger) : RateLimiter
{
    private static readonly TimeSpan WarningInterval = TimeSpan.FromMinutes(1);

    private long _lastWarningTicks;

    public override TimeSpan? IdleDuration
    {
        get
        {
            try
            {
                return redisLimiter.IdleDuration;
            }
            catch (Exception exception) when (IsRedisFailure(exception))
            {
                return localLimiter.IdleDuration;
            }
        }
    }

    public override RateLimiterStatistics? GetStatistics()
    {
        try
        {
            return redisLimiter.GetStatistics();
        }
        catch (Exception exception) when (IsRedisFailure(exception))
        {
            return localLimiter.GetStatistics();
        }
    }

    protected override RateLimitLease AttemptAcquireCore(int permitCount)
    {
        try
        {
            return redisLimiter.AttemptAcquire(permitCount);
        }
        catch (Exception exception) when (IsRedisFailure(exception))
        {
            WarnFallback(exception);
            return localLimiter.AttemptAcquire(permitCount);
        }
    }

    protected override async ValueTask<RateLimitLease> AcquireAsyncCore(int permitCount, CancellationToken cancellationToken)
    {
        try
        {
            return await redisLimiter.AcquireAsync(permitCount, cancellationToken);
        }
        catch (Exception exception) when (IsRedisFailure(exception))
        {
            WarnFallback(exception);
            return await localLimiter.AcquireAsync(permitCount, cancellationToken);
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            redisLimiter.Dispose();
            localLimiter.Dispose();
        }

        base.Dispose(disposing);
    }

    protected override async ValueTask DisposeAsyncCore()
    {
        await redisLimiter.DisposeAsync();
        await localLimiter.DisposeAsync();
    }

    internal static bool IsRedisFailure(Exception exception) =>
        exception is RedisException or TimeoutException or ObjectDisposedException;

    private void WarnFallback(Exception exception)
    {
        // At most one warning per interval: under load every request would otherwise log.
        long now = DateTime.UtcNow.Ticks;
        long last = Interlocked.Read(ref _lastWarningTicks);

        if (now - last < WarningInterval.Ticks ||
            Interlocked.CompareExchange(ref _lastWarningTicks, now, last) != last)
        {
            return;
        }

        logger.LogWarning(
            exception,
            "Redis is unavailable for rate limiting; falling back to the in-process limiter until it recovers.");
    }
}
