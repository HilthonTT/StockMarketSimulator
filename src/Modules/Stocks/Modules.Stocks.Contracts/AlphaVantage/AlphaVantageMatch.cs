using Newtonsoft.Json;

namespace Modules.Stocks.Contracts.AlphaVantage;

public sealed class AlphaVantageMatch
{
    [JsonProperty("1. symbol")]
    public string Symbol { get; set; } = string.Empty;

    [JsonProperty("2. name")]
    public string Name { get; set; } = string.Empty;

    [JsonProperty("3. type")]
    public string Type { get; set; } = string.Empty;

    [JsonProperty("4. region")]
    public string Region { get; set; } = string.Empty;

    [JsonProperty("5. marketOpen")]
    public string MarketOpen { get; set; } = string.Empty;

    [JsonProperty("6. marketClose")]
    public string MarketClose { get; set; } = string.Empty;

    [JsonProperty("7. timezone")]
    public string TimeZone { get; set; } = string.Empty;

    [JsonProperty("8. currency")]
    public string Currency { get; set; } = string.Empty;

    [JsonProperty("9. matchScore")]
    public string MatchScore { get; set; } = string.Empty;
}
