using System.Globalization;
using Modules.Stocks.Domain.Services;

namespace Domain.UnitTests.Stocks;

public sealed class MarketSimulationTests
{
    [Fact]
    public void Catalog_Should_HaveUniqueValidTickers_AndSaneParameters()
    {
        Assert.Equal(
            MarketCatalog.Assets.Count,
            MarketCatalog.Assets.Select(a => a.Ticker).Distinct(StringComparer.OrdinalIgnoreCase).Count());

        Assert.All(MarketCatalog.Assets, asset =>
        {
            Assert.InRange(asset.Ticker.Length, 1, 10);
            Assert.Equal(asset.Ticker.ToUpperInvariant(), asset.Ticker);
            Assert.True(asset.SeedPrice > 0);
            Assert.InRange(asset.Beta, 0, 1);
            Assert.InRange(asset.DailyVolatility, 0.001, 0.1);
            Assert.Contains(asset.AssetClass, new[] { AssetClasses.Stock, AssetClasses.Crypto });
        });

        Assert.Contains(MarketCatalog.Assets, a => a.AssetClass == AssetClasses.Crypto);
    }

    [Fact]
    public void Catalog_IndexDivisor_Should_StartTheIndexAtOneThousand()
    {
        decimal start = MarketCatalog.Assets
            .Where(a => a.AssetClass == AssetClasses.Stock)
            .Sum(a => a.SeedPrice) / MarketCatalog.IndexDivisor;

        Assert.Equal(1000m, Math.Round(start, 6));
    }

    [Fact]
    public void HistoryGenerator_Should_EndExactlyAtTheCurrentPrice_WithConsistentCandles()
    {
        var end = new DateTime(2026, 11, 15, 12, 0, 0, DateTimeKind.Utc);

        IReadOnlyList<PriceCandle> candles = PriceHistoryGenerator.Generate(
            123.45m, end, TimeSpan.FromMinutes(5), 288, 0.0005, 0.02, new Random(7));

        Assert.Equal(288, candles.Count);
        Assert.Equal(123.45m, candles[^1].Close);
        Assert.Equal(end - TimeSpan.FromMinutes(5), candles[^1].Start);
        Assert.All(candles, c =>
        {
            Assert.True(c.Low > 0);
            Assert.True(c.High >= Math.Max(c.Open, c.Low));
            Assert.True(c.Low <= Math.Min(c.Open, c.High));
        });

        // Strictly increasing timestamps, one bar apart.
        Assert.All(candles.Zip(candles.Skip(1)), pair => Assert.Equal(TimeSpan.FromMinutes(5), pair.Second.Start - pair.First.Start));
    }

    // Expected values are strings: since .NET 11 an explicit (decimal)double cast keeps the exact binary value
    // (0.12 -> 0.11999999999999999555...), so a double literal can't express the exact expected decimal.
    [Theory]
    [InlineData(0.12, "0.12")]
    [InlineData(0.123456, "0.1235")]
    [InlineData(12.345000000000001, "12.35")]
    [InlineData(0.00001, "0.0001")]
    public void ToPrice_Should_KeepSubDollarPrecision(double value, string expected)
    {
        Assert.Equal(decimal.Parse(expected, CultureInfo.InvariantCulture), PriceHistoryGenerator.ToPrice(value));
    }

    [Fact]
    public void NewsGenerator_Should_ProduceBoundedImpacts_PerAssetClass()
    {
        var random = new Random(3);
        MarketCatalog.TryGet("AAPL", out ListedAsset stock);
        MarketCatalog.TryGet("DOGE-USD", out ListedAsset coin);

        for (int i = 0; i < 200; i++)
        {
            MarketNewsEvent stockNews = MarketNewsGenerator.Create(stock, random);
            MarketNewsEvent coinNews = MarketNewsGenerator.Create(coin, random);

            Assert.InRange(Math.Abs(stockNews.ImpactPercent), 0.01, 0.03);
            Assert.InRange(Math.Abs(coinNews.ImpactPercent), 0.02, 0.06);
            Assert.Contains(stock.Name, stockNews.Headline, StringComparison.Ordinal);
        }
    }
}
