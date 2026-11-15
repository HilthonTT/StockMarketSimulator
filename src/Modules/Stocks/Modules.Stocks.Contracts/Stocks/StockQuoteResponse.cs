namespace Modules.Stocks.Contracts.Stocks;

/// <summary>
/// A quote with the current session's statistics. <see cref="ChangePercent"/> is a fraction
/// (0.0123 = +1.23%) relative to <see cref="PreviousClose"/>.
/// </summary>
public sealed record StockQuoteResponse(
    string Ticker,
    string? Name,
    string? Sector,
    string AssetClass,
    decimal Price,
    decimal Open,
    decimal PreviousClose,
    decimal DayHigh,
    decimal DayLow,
    decimal Change,
    decimal ChangePercent,
    DateTime UpdatedOnUtc)
{
    public static StockQuoteResponse From(
        string ticker,
        string? name,
        string? sector,
        string assetClass,
        decimal price,
        decimal open,
        decimal previousClose,
        decimal dayHigh,
        decimal dayLow,
        DateTime updatedOnUtc)
    {
        decimal change = price - previousClose;
        decimal changePercent = previousClose == 0 ? 0 : Math.Round(change / previousClose, 6);

        return new StockQuoteResponse(
            ticker, name, sector, assetClass, price, open, previousClose, dayHigh, dayLow,
            change, changePercent, updatedOnUtc);
    }
}
