namespace Modules.Stocks.Contracts.Stocks;

public sealed record StockSearchResponse(
    string Ticker, 
    string Name, 
    string Type, 
    string Region, 
    string MarketOpen,
    string Timezone,
    string Currency);
