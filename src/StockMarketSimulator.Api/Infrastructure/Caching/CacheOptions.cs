using Microsoft.Extensions.Caching.Distributed;

namespace StockMarketSimulator.Api.Infrastructure.Caching;

internal static class CacheOptions
{
    private readonly static DistributedCacheEntryOptions DefaultExpiration = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3)
    };

    public static DistributedCacheEntryOptions Create(TimeSpan? expiration)
    {
        if (expiration is not null)
        {
            return new DistributedCacheEntryOptions()
            {
                AbsoluteExpirationRelativeToNow = expiration
            };
        }

        return DefaultExpiration;
    }
}
