namespace Modules.Stocks.Domain.Services;

/// <summary>A generated news event and the price shock it causes.</summary>
/// <param name="ImpactPercent">Relative price jump as a fraction, e.g. <c>0.04</c> for +4%.</param>
public sealed record MarketNewsEvent(string Ticker, string Headline, double ImpactPercent);

/// <summary>
/// Occasionally produces company news that moves a single asset by a few percent, on top of the
/// regular random walk. It's what makes the market feel eventful instead of a smooth wiggle.
/// </summary>
public static class MarketNewsGenerator
{
    private static readonly string[] PositiveStockHeadlines =
    [
        "{0} beats quarterly earnings estimates",
        "{0} raises full-year guidance",
        "Analysts upgrade {0} to Buy",
        "{0} announces $10B share buyback",
        "{0} unveils new flagship product to strong reviews",
        "{0} lands major government contract",
        "{0} reports record revenue",
    ];

    private static readonly string[] NegativeStockHeadlines =
    [
        "{0} misses quarterly earnings estimates",
        "{0} cuts full-year guidance",
        "Analysts downgrade {0} to Sell",
        "Regulators open probe into {0}",
        "{0} recalls products over safety concerns",
        "{0} CFO unexpectedly steps down",
        "{0} warns of supply chain disruption",
    ];

    private static readonly string[] PositiveCryptoHeadlines =
    [
        "{0} rallies as exchange inflows surge",
        "Major fund adds {0} to its treasury",
        "{0} network upgrade goes live without issues",
        "{0} ETF sees record daily inflows",
    ];

    private static readonly string[] NegativeCryptoHeadlines =
    [
        "{0} slides after large whale sell-off",
        "Exchange halts {0} withdrawals temporarily",
        "{0} network suffers brief outage",
        "Regulators signal tougher stance on {0}",
    ];

    /// <summary>
    /// Builds a news event for <paramref name="asset"/>: positive or negative with equal odds, with a jump
    /// of 1-3% (stocks) or 2-6% (crypto).
    /// </summary>
    public static MarketNewsEvent Create(ListedAsset asset, Random random)
    {
        bool positive = random.NextDouble() < 0.5;
        bool crypto = asset.AssetClass == AssetClasses.Crypto;

        string[] templates = (positive, crypto) switch
        {
            (true, false) => PositiveStockHeadlines,
            (false, false) => NegativeStockHeadlines,
            (true, true) => PositiveCryptoHeadlines,
            (false, true) => NegativeCryptoHeadlines,
        };

        double minimum = crypto ? 0.02 : 0.01;
        double magnitude = minimum + (random.NextDouble() * minimum * 2);

        string headline = string.Format(
            System.Globalization.CultureInfo.InvariantCulture,
            templates[random.Next(templates.Length)],
            asset.Name);

        return new MarketNewsEvent(asset.Ticker, headline, positive ? magnitude : -magnitude);
    }
}
