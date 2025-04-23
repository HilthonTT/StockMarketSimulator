using Application.Abstractions.Caching;
using Modules.Stocks.Contracts.Shorten;

namespace Modules.Stocks.Application.Shorten.GetByTicker;

public sealed record GetShortenUrlByTickerQuery(string Ticker) : ICachedQuery<ShortenUrlResponse>
{
    public string CacheKey => $"shorten:{Ticker}";

    public TimeSpan? Expiration => null;
}
