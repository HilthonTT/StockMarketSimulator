using Modules.Stocks.Application.Abstractions.Realtime;

namespace Modules.Stocks.Infrastructure.Realtime;

/// <summary>
/// Provides static volatility parameters (μ, σ) for known stock tickers.
/// Defaults are used for unknown tickers.
/// </summary>
internal sealed class SimpleStockVolatilityProvider : IStockVolatilityProvider
{
    /// <summary>
    /// Represents expected return (Mu) and volatility (Sigma) for a stock.
    /// </summary>
    private sealed record VolatilityProfile(double Mu, double Sigma);

    private static readonly Dictionary<string, VolatilityProfile> _parameters = new()
    {
        // Tech
        ["AAPL"] = new(0.0006, 0.035),
        ["MSFT"] = new(0.00055, 0.03),
        ["GOOG"] = new(0.0006, 0.04),
        ["TSLA"] = new(0.0008, 0.05),
        ["NVDA"] = new(0.0007, 0.045),
        ["AMD"] = new(0.0007, 0.05),
        ["META"] = new(0.00065, 0.042),
        ["INTC"] = new(0.0005, 0.028),
        ["CRM"] = new(0.00055, 0.03),
        ["ADBE"] = new(0.0006, 0.034),

        // Finance
        ["JPM"] = new(0.0004, 0.02),
        ["BAC"] = new(0.0004, 0.022),
        ["WFC"] = new(0.00038, 0.021),
        ["C"] = new(0.00039, 0.023),
        ["GS"] = new(0.00042, 0.024),
        ["MS"] = new(0.00041, 0.022),
        ["SCHW"] = new(0.0004, 0.02),
        ["AXP"] = new(0.00043, 0.019),
        ["USB"] = new(0.00038, 0.02),
        ["PNC"] = new(0.00037, 0.019),

        // Energy
        ["XOM"] = new(0.00035, 0.018),
        ["CVX"] = new(0.00035, 0.017),
        ["SLB"] = new(0.00036, 0.02),
        ["COP"] = new(0.00037, 0.021),
        ["EOG"] = new(0.00036, 0.019),
        ["PSX"] = new(0.00034, 0.017),
        ["VLO"] = new(0.00035, 0.018),
        ["HAL"] = new(0.00037, 0.022),
        ["BKR"] = new(0.00036, 0.02),
        ["MPC"] = new(0.00035, 0.019),

        // Utilities
        ["NEE"] = new(0.0003, 0.015),
        ["DUK"] = new(0.00028, 0.014),
        ["SO"] = new(0.00027, 0.013),
        ["D"] = new(0.00029, 0.015),
        ["AEP"] = new(0.00028, 0.014),
        ["EXC"] = new(0.00027, 0.013),
        ["ED"] = new(0.00026, 0.012),
        ["PEG"] = new(0.00027, 0.014),
        ["WEC"] = new(0.00027, 0.013),
        ["XEL"] = new(0.00028, 0.013),

        // Consumer
        ["AMZN"] = new(0.00065, 0.04),
        ["WMT"] = new(0.0005, 0.025),
        ["COST"] = new(0.00048, 0.022),
        ["TGT"] = new(0.00046, 0.023),
        ["HD"] = new(0.0005, 0.026),
        ["LOW"] = new(0.00049, 0.025),
        ["PG"] = new(0.00045, 0.02),
        ["KO"] = new(0.00044, 0.018),
        ["PEP"] = new(0.00043, 0.018),
        ["MCD"] = new(0.00046, 0.019)
    };

    private static readonly VolatilityProfile _default = new(0.0005, 0.025);

    /// <summary>
    /// Gets the expected return and volatility parameters for a stock.
    /// </summary>
    /// <param name="ticker">The stock ticker symbol.</param>
    /// <returns>A tuple containing (Mu, Sigma) values.</returns>
    public (double Mu, double Sigma) GetParameters(string ticker)
    {
        VolatilityProfile profile = _parameters.TryGetValue(ticker, out var value) ? value : _default;

        return (profile.Mu, profile.Sigma);
    }
}
