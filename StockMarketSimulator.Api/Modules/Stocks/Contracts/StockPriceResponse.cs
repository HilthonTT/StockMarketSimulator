namespace StockMarketSimulator.Api.Modules.Stocks.Contracts;

public sealed record StockPriceResponse(string Ticker, decimal Price);
