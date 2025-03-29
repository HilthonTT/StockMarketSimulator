using Newtonsoft.Json;

namespace Modules.Stocks.Contracts.AlphaVantage;

public sealed class AlphaVantageTimeSeriesEntry
{
    [JsonProperty("1. open")]
    public string Open { get; set; } = string.Empty;

    [JsonProperty("2. high")]
    public string High { get; set; } = string.Empty;

    [JsonProperty("3. low")]
    public string Low { get; set; } = string.Empty;

    [JsonProperty("4. close")]
    public string Close { get; set; } = string.Empty;

    [JsonProperty("5. volume")]
    public string Volume { get; set; } = string.Empty;
}
