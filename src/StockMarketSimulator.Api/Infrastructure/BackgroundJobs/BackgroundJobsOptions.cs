namespace StockMarketSimulator.Api.Infrastructure.BackgroundJobs;

public sealed class BackgroundJobsOptions
{
    public const string ConfigurationSection = "BackgroundJobsOptions";

    public int IntervalInSeconds { get; set; }
}
