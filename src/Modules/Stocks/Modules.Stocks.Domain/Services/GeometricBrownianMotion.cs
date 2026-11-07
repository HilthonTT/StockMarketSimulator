namespace Modules.Stocks.Domain.Services;

/// <summary>
/// Geometric Brownian Motion price model used by the real-time feed.
/// </summary>
/// <remarks>
/// For a step of length <c>dt</c> (in trading days) the exact GBM update is
/// <c>P' = P * exp((mu - sigma^2 / 2) * dt + sigma * sqrt(dt) * Z)</c>, with <c>Z ~ N(0, 1)</c>.
/// <c>mu</c> and <c>sigma</c> are <em>daily</em> drift and volatility. The per-step relative change is
/// clamped to <c>maxStepChange</c> so a single tick can never move the price absurdly.
/// The method is pure (the random draw is passed in), which keeps it deterministic and testable.
/// </remarks>
public static class GeometricBrownianMotion
{
    /// <summary>Length of a regular US trading session in seconds (6.5 hours).</summary>
    public const double TradingSecondsPerDay = 6.5 * 60 * 60;

    public static decimal NextPrice(
        decimal currentPrice,
        double dailyDrift,
        double dailyVolatility,
        double stepInTradingDays,
        double maxStepChange,
        double standardNormal)
    {
        if (currentPrice <= 0)
        {
            return currentPrice;
        }

        double exponent =
            ((dailyDrift - (dailyVolatility * dailyVolatility / 2)) * stepInTradingDays) +
            (dailyVolatility * Math.Sqrt(stepInTradingDays) * standardNormal);

        double relativeChange = Math.Clamp(Math.Exp(exponent) - 1, -maxStepChange, maxStepChange);

        decimal nextPrice = currentPrice * (decimal)(1 + relativeChange);

        // Keep a strictly positive, cent-rounded price.
        return Math.Max(Math.Round(nextPrice, 2, MidpointRounding.AwayFromZero), 0.01m);
    }

    /// <summary>Draws a standard normal value using the Box-Muller transform.</summary>
    public static double SampleStandardNormal(Random random)
    {
        double u1 = 1.0 - random.NextDouble(); // (0, 1]
        double u2 = 1.0 - random.NextDouble();

        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
    }
}
