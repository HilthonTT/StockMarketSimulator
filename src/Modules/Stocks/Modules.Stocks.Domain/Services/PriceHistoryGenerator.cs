namespace Modules.Stocks.Domain.Services;

/// <summary>An OHLC candle starting at <see cref="Start"/>.</summary>
public sealed record PriceCandle(DateTime Start, decimal Open, decimal High, decimal Low, decimal Close);

/// <summary>
/// Builds a plausible price history that <em>ends</em> at a given price, so a freshly listed asset
/// has a chart from the first second instead of an empty one.
/// </summary>
/// <remarks>
/// A GBM path is generated forward from an arbitrary start and then rescaled so its last close equals
/// the current price. Rescaling a GBM path by a constant keeps its returns (and so its volatility).
/// Time is treated as wall-clock time scaled to trading days, exactly like the live feed.
/// </remarks>
public static class PriceHistoryGenerator
{
    /// <summary>Number of intra-bar sub-steps used to derive the high and the low of each candle.</summary>
    private const int SubSteps = 4;

    public static IReadOnlyList<PriceCandle> Generate(
        decimal endPrice,
        DateTime endUtc,
        TimeSpan barLength,
        int barCount,
        double dailyDrift,
        double dailyVolatility,
        Random random)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(endPrice);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(barCount);

        double stepInTradingDays = barLength.TotalSeconds / SubSteps / GeometricBrownianMotion.TradingSecondsPerDay;
        double drift = (dailyDrift - (dailyVolatility * dailyVolatility / 2)) * stepInTradingDays;
        double diffusion = dailyVolatility * Math.Sqrt(stepInTradingDays);

        var raw = new (double Open, double High, double Low, double Close)[barCount];
        double price = 1.0;

        for (int i = 0; i < barCount; i++)
        {
            double open = price;
            double high = price;
            double low = price;

            for (int s = 0; s < SubSteps; s++)
            {
                price *= Math.Exp(drift + (diffusion * GeometricBrownianMotion.SampleStandardNormal(random)));
                high = Math.Max(high, price);
                low = Math.Min(low, price);
            }

            raw[i] = (open, high, low, price);
        }

        double scale = (double)endPrice / raw[^1].Close;
        DateTime firstStart = endUtc - (barLength * barCount);
        var candles = new List<PriceCandle>(barCount);

        for (int i = 0; i < barCount; i++)
        {
            (double open, double high, double low, double close) = raw[i];

            candles.Add(new PriceCandle(
                firstStart + (barLength * i),
                ToPrice(open * scale),
                ToPrice(high * scale),
                ToPrice(low * scale),
                // The last close is exactly the current price, not a rounding of it.
                i == barCount - 1 ? endPrice : ToPrice(close * scale)));
        }

        return candles;
    }

    /// <summary>Rounds to cents above $1 and to 4 decimals below (cheap crypto), never below the minimum.</summary>
    public static decimal ToPrice(double value)
    {
        decimal price = (decimal)value;
        int decimals = price >= 1m ? 2 : 4;

        return Math.Max(Math.Round(price, decimals, MidpointRounding.AwayFromZero), MinimumPrice(price));
    }

    private static decimal MinimumPrice(decimal price) => price >= 1m ? 0.01m : 0.0001m;
}
