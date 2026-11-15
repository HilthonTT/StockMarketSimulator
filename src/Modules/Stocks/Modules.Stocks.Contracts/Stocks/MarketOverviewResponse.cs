namespace Modules.Stocks.Contracts.Stocks;

public sealed record MarketIndexResponse(string Name, decimal Value, decimal Change, decimal ChangePercent);

/// <param name="Sparklines">Per ticker, the closes of the last 24 hours in 30-minute steps (oldest first).</param>
public sealed record MarketOverviewResponse(
    MarketIndexResponse Index,
    IReadOnlyList<StockQuoteResponse> Quotes,
    IReadOnlyDictionary<string, decimal[]> Sparklines);
