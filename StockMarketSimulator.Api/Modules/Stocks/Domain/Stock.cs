namespace StockMarketSimulator.Api.Modules.Stocks.Domain;

internal sealed class Stock
{
    public required Guid Id { get; set; }

    public required string Ticker { get; set; }

    public required decimal Price { get; set; }

    public required DateTime Timestamp { get; set; }
}
