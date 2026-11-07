using System.Threading.RateLimiting;
using Microsoft.Extensions.Logging.Abstractions;
using StackExchange.Redis;
using Web.Api.RateLimiting;

namespace Application.UnitTests.Web;

public sealed class RedisFallbackRateLimiterTests
{
    [Fact]
    public async Task AcquireAsync_Should_FallBackToLocalLimiter_WhenRedisIsUnavailable()
    {
        using var limiter = new RedisFallbackRateLimiter(
            new ThrowingLimiter(),
            GlobalLimiterFactory.CreateLocal(),
            NullLogger.Instance);

        using RateLimitLease lease = await limiter.AcquireAsync();

        Assert.True(lease.IsAcquired);
    }

    [Fact]
    public async Task AcquireAsync_Should_StillLimit_InFallbackMode()
    {
        using var limiter = new RedisFallbackRateLimiter(
            new ThrowingLimiter(),
            GlobalLimiterFactory.CreateLocal(),
            NullLogger.Instance);

        var leases = new List<RateLimitLease>();
        for (int i = 0; i < GlobalLimiterFactory.TokenLimit; i++)
        {
            leases.Add(await limiter.AcquireAsync());
        }

        using RateLimitLease rejected = await limiter.AcquireAsync();

        Assert.All(leases, lease => Assert.True(lease.IsAcquired));
        Assert.False(rejected.IsAcquired);
        leases.ForEach(lease => lease.Dispose());
    }

    [Fact]
    public void Create_Should_ReturnInProcessLimiter_WhenRedisIsNotConfigured()
    {
        using RateLimiter limiter = GlobalLimiterFactory.Create("global", connectionMultiplexer: null, NullLogger.Instance);

        Assert.IsType<TokenBucketRateLimiter>(limiter);
        using RateLimitLease lease = limiter.AttemptAcquire();
        Assert.True(lease.IsAcquired);
    }

    private sealed class ThrowingLimiter : RateLimiter
    {
        public override TimeSpan? IdleDuration => throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, "down");

        public override RateLimiterStatistics? GetStatistics() =>
            throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, "down");

        protected override RateLimitLease AttemptAcquireCore(int permitCount) =>
            throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, "down");

        protected override ValueTask<RateLimitLease> AcquireAsyncCore(int permitCount, CancellationToken cancellationToken) =>
            throw new RedisConnectionException(ConnectionFailureType.UnableToConnect, "down");
    }
}
