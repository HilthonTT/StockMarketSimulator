namespace StockMarketSimulator.Api.Modules.Transactions.Domain;

internal sealed class Transaction
{
    public required Guid Id { get; set; }

    public required Guid UserId { get; set; }

    public required string Ticker { get; set; }

    public required decimal LimitPrice { get; set; }

    public required TransactionType Type { get; set; }

    public required int Quantity { get; set; }

    public decimal TotalAmount => LimitPrice * Quantity;

    public required DateTime CreatedOnUtc { get; set; }
}
