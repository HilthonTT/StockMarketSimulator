namespace Modules.Stocks.Contracts.Stocks;

/// <summary>A live tick pushed over SignalR (<c>ReceiveStockPriceUpdate</c>).</summary>
public sealed record StockPriceUpdate(
    string Ticker,
    decimal Price,
    decimal Open,
    decimal PreviousClose,
    decimal DayHigh,
    decimal DayLow,
    DateTime Timestamp);
