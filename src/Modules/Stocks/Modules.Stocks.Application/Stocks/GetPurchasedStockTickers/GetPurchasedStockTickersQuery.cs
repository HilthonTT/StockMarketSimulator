using Application.Abstractions.Caching;
using Modules.Stocks.Contracts.Stocks;

namespace Modules.Stocks.Application.Stocks.GetPurchasedStockTickers;

public sealed record GetPurchasedStockTickersQuery(Guid UserId) : ICachedQuery<PurchasedStockTickersResponse>
{
    public string CacheKey => $"users:{UserId}:purchased-stock-tickers";

    public TimeSpan? Expiration => null;
}
