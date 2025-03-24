using SharedKernel;

namespace StockMarketSimulator.Api.Modules.Transactions.Domain;

internal sealed class Transaction : IEntity
{
    private Transaction(
        Guid id,
        Guid userId, 
        string ticker, 
        decimal limitPrice, 
        TransactionType type, 
        int quantity, 
        DateTime createdOnUtc)
    {
        Ensure.NotNullOrEmpty(id, nameof(id));
        Ensure.NotNullOrEmpty(userId, nameof(userId));
        Ensure.NotNullOrEmpty(ticker, nameof(ticker));
        Ensure.GreaterThanOrEqualToZero(limitPrice, nameof(limitPrice));
        Ensure.GreaterThanOrEqualToZero(quantity, nameof(quantity));
        Ensure.NotNull(createdOnUtc, nameof(createdOnUtc));

        Id = id;
        UserId = userId;
        Ticker = ticker;
        LimitPrice = limitPrice;
        Type = type;
        Quantity = quantity;
        CreatedOnUtc = createdOnUtc;
    }

    private Transaction()
    {
    }

    public Guid Id { get; private set; }

    public Guid UserId { get; private set; }

    public string Ticker { get; private set; }

    public decimal LimitPrice { get; private set; }

    public TransactionType Type { get; private set; }

    public int Quantity { get; private set; }

    public DateTime CreatedOnUtc { get; private set; }

    public decimal TotalAmount => LimitPrice * Quantity;

    public static Transaction Create(Guid userId, string ticker, decimal limitPrice, TransactionType type, int quantity)
    {
        return new Transaction(Guid.NewGuid(), userId, ticker, limitPrice, type, quantity, DateTime.UtcNow);
    }
}