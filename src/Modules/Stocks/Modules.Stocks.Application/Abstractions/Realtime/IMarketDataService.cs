using Modules.Stocks.Contracts.Stocks;

namespace Modules.Stocks.Application.Abstractions.Realtime;

/// <summary>Price history and market-wide reads.</summary>
public interface IMarketDataService
{
    /// <summary>Quotes of the built-in universe plus the index computed from them.</summary>
    Task<MarketOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken = default);

    /// <summary>OHLC history for a supported range, re-bucketed to a chart-friendly resolution.</summary>
    Task<PriceHistoryResponse> GetHistoryAsync(string ticker, PriceRange range, CancellationToken cancellationToken = default);

    /// <summary>Current prices of the given tickers (unknown tickers are skipped).</summary>
    Task<IReadOnlyDictionary<string, decimal>> GetPricesAsync(
        IReadOnlyCollection<string> tickers,
        CancellationToken cancellationToken = default);
}

/// <summary>A chart range with the bucket size its points are aggregated to.</summary>
public sealed record PriceRange(string Name, TimeSpan Span, TimeSpan Bucket)
{
    public static readonly PriceRange OneDay = new("1D", TimeSpan.FromDays(1), TimeSpan.FromMinutes(5));
    public static readonly PriceRange OneWeek = new("1W", TimeSpan.FromDays(7), TimeSpan.FromMinutes(30));
    public static readonly PriceRange OneMonth = new("1M", TimeSpan.FromDays(30), TimeSpan.FromHours(2));
    public static readonly PriceRange ThreeMonths = new("3M", TimeSpan.FromDays(90), TimeSpan.FromHours(8));

    public static readonly IReadOnlyList<PriceRange> All = [OneDay, OneWeek, OneMonth, ThreeMonths];

    public static PriceRange? FromName(string? name) =>
        All.FirstOrDefault(r => string.Equals(r.Name, name, StringComparison.OrdinalIgnoreCase));
}
