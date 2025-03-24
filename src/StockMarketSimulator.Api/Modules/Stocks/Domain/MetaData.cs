using Newtonsoft.Json;

namespace StockMarketSimulator.Api.Modules.Stocks.Domain;

internal sealed class MetaData
{
    [JsonProperty("1. Information")]
    public required string Information { get; set; }

    [JsonProperty("2. Symbol")]
    public required string Symbol { get; set; }

    [JsonProperty("3. Last Refreshed")]
    public required string LastRefreshed { get; set; }

    [JsonProperty("4. Interval")]
    public required string Interval { get; set; }

    [JsonProperty("5. Output Size")]
    public required string OutputSize { get; set; }

    [JsonProperty("6. Time Zone")]
    public required string TimeZone { get; set; }
}
