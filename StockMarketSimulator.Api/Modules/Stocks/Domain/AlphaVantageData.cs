using Newtonsoft.Json;

namespace StockMarketSimulator.Api.Modules.Stocks.Domain;

internal sealed class AlphaVantageData
{
    [JsonProperty("Meta Data")]
    public required MetaData MetaData { get; set; }

    [JsonProperty("Time Series (15min)")]
    public required Dictionary<string, TimeSeriesEntry> TimeSeries { get; set; } = [];
}
