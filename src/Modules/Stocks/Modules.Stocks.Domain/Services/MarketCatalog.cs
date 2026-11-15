namespace Modules.Stocks.Domain.Services;

/// <summary>The kind of asset a listing represents.</summary>
public static class AssetClasses
{
    public const string Stock = "stock";

    public const string Crypto = "crypto";
}

/// <summary>
/// A listing in the built-in market universe.
/// </summary>
/// <param name="Ticker">Upper-case symbol, e.g. <c>AAPL</c> or <c>ETH-USD</c>.</param>
/// <param name="SeedPrice">Starting price used when the listing is first created.</param>
/// <param name="DailyDrift">GBM drift (mu) per trading day.</param>
/// <param name="DailyVolatility">GBM volatility (sigma) per trading day.</param>
/// <param name="Beta">
/// Sensitivity to the shared market factor (0..1). Listings with a high beta move together,
/// which is what makes the market rise and fall as a whole instead of as independent noise.
/// </param>
public sealed record ListedAsset(
    string Ticker,
    string Name,
    string Sector,
    string AssetClass,
    decimal SeedPrice,
    double DailyDrift,
    double DailyVolatility,
    double Beta);

/// <summary>
/// The simulator's built-in universe: well-known stocks and crypto assets that are always
/// listed and ticked by the live feed, so the market works without any external price API.
/// </summary>
/// <remarks>
/// Seed prices are only starting points; from there every price is a simulated random walk.
/// Crypto trades in whole units like stocks, so assets priced far above the starting cash
/// (e.g. BTC) are intentionally not listed.
/// </remarks>
public static class MarketCatalog
{
    /// <summary>Name of the price-weighted index over all listed stocks.</summary>
    public const string IndexName = "SIM 30";

    public static readonly IReadOnlyList<ListedAsset> Assets =
    [
        // Technology
        new("AAPL", "Apple Inc.", "Technology", AssetClasses.Stock, 228.50m, 0.0006, 0.018, 0.85),
        new("MSFT", "Microsoft Corp.", "Technology", AssetClasses.Stock, 421.30m, 0.00055, 0.016, 0.85),
        new("NVDA", "NVIDIA Corp.", "Technology", AssetClasses.Stock, 138.20m, 0.0009, 0.032, 0.8),
        new("GOOGL", "Alphabet Inc.", "Technology", AssetClasses.Stock, 172.60m, 0.0006, 0.019, 0.8),
        new("META", "Meta Platforms Inc.", "Technology", AssetClasses.Stock, 562.10m, 0.0007, 0.024, 0.8),
        new("AMD", "Advanced Micro Devices", "Technology", AssetClasses.Stock, 152.40m, 0.0007, 0.03, 0.75),
        new("INTC", "Intel Corp.", "Technology", AssetClasses.Stock, 22.80m, 0.0002, 0.026, 0.7),
        new("CRM", "Salesforce Inc.", "Technology", AssetClasses.Stock, 291.70m, 0.0005, 0.021, 0.75),

        // Consumer
        new("AMZN", "Amazon.com Inc.", "Consumer", AssetClasses.Stock, 186.90m, 0.00065, 0.021, 0.8),
        new("TSLA", "Tesla Inc.", "Consumer", AssetClasses.Stock, 248.30m, 0.0008, 0.038, 0.7),
        new("WMT", "Walmart Inc.", "Consumer", AssetClasses.Stock, 80.40m, 0.0004, 0.011, 0.5),
        new("KO", "Coca-Cola Co.", "Consumer", AssetClasses.Stock, 68.90m, 0.0003, 0.009, 0.4),
        new("MCD", "McDonald's Corp.", "Consumer", AssetClasses.Stock, 296.20m, 0.0003, 0.011, 0.45),
        new("NKE", "Nike Inc.", "Consumer", AssetClasses.Stock, 79.60m, 0.0003, 0.02, 0.6),
        new("DIS", "Walt Disney Co.", "Communication", AssetClasses.Stock, 96.10m, 0.0004, 0.017, 0.65),
        new("NFLX", "Netflix Inc.", "Communication", AssetClasses.Stock, 712.40m, 0.0007, 0.025, 0.7),

        // Financials
        new("JPM", "JPMorgan Chase & Co.", "Financials", AssetClasses.Stock, 214.70m, 0.0004, 0.014, 0.75),
        new("BAC", "Bank of America Corp.", "Financials", AssetClasses.Stock, 40.20m, 0.0004, 0.017, 0.75),
        new("GS", "Goldman Sachs Group", "Financials", AssetClasses.Stock, 512.80m, 0.00045, 0.017, 0.75),
        new("V", "Visa Inc.", "Financials", AssetClasses.Stock, 279.30m, 0.0005, 0.012, 0.65),

        // Healthcare
        new("JNJ", "Johnson & Johnson", "Healthcare", AssetClasses.Stock, 158.40m, 0.0003, 0.01, 0.4),
        new("PFE", "Pfizer Inc.", "Healthcare", AssetClasses.Stock, 28.60m, 0.0002, 0.015, 0.45),
        new("UNH", "UnitedHealth Group", "Healthcare", AssetClasses.Stock, 588.20m, 0.0004, 0.016, 0.5),
        new("LLY", "Eli Lilly and Co.", "Healthcare", AssetClasses.Stock, 902.50m, 0.0008, 0.022, 0.5),

        // Energy & industrials
        new("XOM", "Exxon Mobil Corp.", "Energy", AssetClasses.Stock, 117.30m, 0.0003, 0.015, 0.55),
        new("CVX", "Chevron Corp.", "Energy", AssetClasses.Stock, 152.60m, 0.0003, 0.015, 0.55),
        new("BA", "Boeing Co.", "Industrials", AssetClasses.Stock, 156.80m, 0.0003, 0.024, 0.7),
        new("CAT", "Caterpillar Inc.", "Industrials", AssetClasses.Stock, 382.90m, 0.0005, 0.017, 0.7),
        new("NEE", "NextEra Energy Inc.", "Utilities", AssetClasses.Stock, 76.20m, 0.0003, 0.013, 0.35),
        new("DUK", "Duke Energy Corp.", "Utilities", AssetClasses.Stock, 112.40m, 0.00025, 0.01, 0.3),

        // Crypto (trades 24/7, much higher volatility, loosely tied to the stock market factor)
        new("ETH-USD", "Ethereum", "Crypto", AssetClasses.Crypto, 2640.00m, 0.0008, 0.042, 0.35),
        new("SOL-USD", "Solana", "Crypto", AssetClasses.Crypto, 148.50m, 0.001, 0.055, 0.35),
        new("BNB-USD", "BNB", "Crypto", AssetClasses.Crypto, 584.00m, 0.0007, 0.035, 0.3),
        new("XRP-USD", "XRP", "Crypto", AssetClasses.Crypto, 0.58m, 0.0007, 0.048, 0.3),
        new("ADA-USD", "Cardano", "Crypto", AssetClasses.Crypto, 0.36m, 0.0007, 0.05, 0.3),
        new("DOGE-USD", "Dogecoin", "Crypto", AssetClasses.Crypto, 0.12m, 0.0009, 0.065, 0.3),
        new("AVAX-USD", "Avalanche", "Crypto", AssetClasses.Crypto, 27.40m, 0.0008, 0.058, 0.35),
        new("LINK-USD", "Chainlink", "Crypto", AssetClasses.Crypto, 11.80m, 0.0008, 0.052, 0.35),
        new("DOT-USD", "Polkadot", "Crypto", AssetClasses.Crypto, 4.60m, 0.0006, 0.05, 0.3),
        new("LTC-USD", "Litecoin", "Crypto", AssetClasses.Crypto, 68.30m, 0.0005, 0.04, 0.3),
    ];

    private static readonly Dictionary<string, ListedAsset> ByTicker =
        Assets.ToDictionary(asset => asset.Ticker, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Divisor of the price-weighted index, chosen so the index starts at 1,000 with the seed prices.
    /// </summary>
    public static readonly decimal IndexDivisor =
        Assets.Where(a => a.AssetClass == AssetClasses.Stock).Sum(a => a.SeedPrice) / 1000m;

    public static IEnumerable<string> Tickers => ByTicker.Keys;

    public static bool TryGet(string ticker, out ListedAsset asset) =>
        ByTicker.TryGetValue(ticker.Trim(), out asset!);

    public static bool IsListed(string ticker) => ByTicker.ContainsKey(ticker.Trim());
}
