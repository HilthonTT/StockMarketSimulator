namespace Modules.Stocks.Infrastructure.Realtime.Options;

public sealed class StockUpdateOptions
{
    public const string ConfigurationSection = "StockUpdateOptions";

    public int UpdateIntervalInSeconds { get; set; } = 1;

    public double MaxPercentageChange { get; set; } = 0.02; // 2%

    public double Volatility { get; set; } = 0.01; // 1% daily volatility
}
