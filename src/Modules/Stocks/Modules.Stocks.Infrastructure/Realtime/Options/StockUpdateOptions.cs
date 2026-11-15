namespace Modules.Stocks.Infrastructure.Realtime.Options;

public sealed class StockUpdateOptions
{
    public const string ConfigurationSection = "StockUpdateOptions";

    public int UpdateIntervalInSeconds { get; set; } = 1;

    public double MaxPercentageChange { get; set; } = 0.02; // 2%

    /// <summary>Average number of simulated news events (price shocks) per minute across the market.</summary>
    public double NewsEventsPerMinute { get; set; } = 0.25;
}
