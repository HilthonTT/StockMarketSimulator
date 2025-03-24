using StockMarketSimulator.Api.Modules.Transactions.Domain;

namespace StockMarketSimulator.Api.Modules.Transactions.Contracts;

internal sealed record TransactionResponse
{
    public Guid Id { get; init; }

    public Guid UserId { get; init; }

    public string Ticker { get; init; } = string.Empty;

    public decimal LimitPrice { get; init; }

    public TransactionType Type { get; init; }

    public int Quantity { get; init; }

    public decimal TotalAmount => LimitPrice * Quantity;

    public DateTime CreatedOnUtc { get; init; }
}
