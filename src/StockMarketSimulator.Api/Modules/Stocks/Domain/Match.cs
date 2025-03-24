using Newtonsoft.Json;

namespace StockMarketSimulator.Api.Modules.Stocks.Domain;

public sealed class Match
{
    [JsonProperty("1. symbol")]
    public required string Symbol { get; set; }

    [JsonProperty("2. name")]
    public required string Name { get; set; }

    [JsonProperty("3. type")]
    public required string Type { get; set; }

    [JsonProperty("4. region")]
    public required string Region { get; set; }

    [JsonProperty("5. marketOpen")]
    public required string MarketOpen { get; set; }

    [JsonProperty("6. marketClose")]
    public required string MarketClose { get; set; }

    [JsonProperty("7. timezone")]
    public required string TimeZone { get; set; }

    [JsonProperty("8. currency")]
    public required string Currency { get; set; }

    [JsonProperty("9. matchScore")]
    public required string MatchScore { get; set; }
}
