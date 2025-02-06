using Newtonsoft.Json;

namespace StockMarketSimulator.Api.Modules.Stocks.Domain;

public sealed class AlphaVantageSearchData
{
    [JsonProperty("bestMatches")]
    public required List<Match> BestMatches { get; set; } = [];
}
