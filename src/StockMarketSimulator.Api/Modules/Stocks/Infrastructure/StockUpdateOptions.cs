namespace StockMarketSimulator.Api.Modules.Stocks.Infrastructure;

internal sealed class StockUpdateOptions
{
    public const string ConfigurationSection = "StockUpdateOptions";

    public int UpdateIntervalInSeconds { get; set; } = 1;

    public double MaxPercentageChange { get; set; } = 0.02; // 2%
}
