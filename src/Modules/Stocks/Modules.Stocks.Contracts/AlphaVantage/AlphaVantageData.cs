using Newtonsoft.Json;

namespace Modules.Stocks.Contracts.AlphaVantage;

public sealed class AlphaVantageData
{
    [JsonProperty("Meta Data")]
    public AlphaVantageMetaData MetaData { get; set; } = null!;

    [JsonProperty("Time Series (15min)")]
    public Dictionary<string, AlphaVantageTimeSeriesEntry> TimeSeries { get; set; } = [];
}
