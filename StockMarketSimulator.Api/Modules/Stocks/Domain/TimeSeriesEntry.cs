using Newtonsoft.Json;

namespace StockMarketSimulator.Api.Modules.Stocks.Domain;

internal sealed class TimeSeriesEntry
{
    [JsonProperty("1. open")]
    public required string Open { get; set; }

    [JsonProperty("2. high")]
    public required string High { get; set; }

    [JsonProperty("3. low")]
    public required string Low { get; set; }

    [JsonProperty("4. close")]
    public required string Close { get; set; }

    [JsonProperty("5. volume")]
    public required string Volume { get; set; }
}
