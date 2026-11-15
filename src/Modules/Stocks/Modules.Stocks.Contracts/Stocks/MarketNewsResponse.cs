namespace Modules.Stocks.Contracts.Stocks;

/// <summary>A simulated news event. <see cref="ImpactPercent"/> is the price jump it caused, as a fraction.</summary>
public sealed record MarketNewsResponse(
    Guid Id,
    string Ticker,
    string Headline,
    decimal ImpactPercent,
    string Sentiment,
    DateTime PublishedOnUtc);
