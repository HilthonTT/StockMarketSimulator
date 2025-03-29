using Newtonsoft.Json;

namespace Modules.Stocks.Contracts.AlphaVantage;

public sealed class AlphaVantageSearchData
{
    [JsonProperty("bestMatches")]
    public List<AlphaVantageMatch> BestMatches { get; set; } = [];
}
