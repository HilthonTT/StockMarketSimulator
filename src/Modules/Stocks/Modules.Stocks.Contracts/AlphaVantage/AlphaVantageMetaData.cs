using Newtonsoft.Json;

namespace Modules.Stocks.Contracts.AlphaVantage;

public sealed class AlphaVantageMetaData
{
    [JsonProperty("1. Information")]
    public string Information { get; set; } = string.Empty;

    [JsonProperty("2. Symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonProperty("3. Last Refreshed")]
    public string LastRefreshed { get; set; } = string.Empty;

    [JsonProperty("4. Interval")]
    public string Interval { get; set; } = string.Empty;

    [JsonProperty("5. Output Size")]
    public string OutputSize { get; set; } = string.Empty;

    [JsonProperty("6. Time Zone")]
    public string TimeZone { get; set; } = string.Empty;
}
