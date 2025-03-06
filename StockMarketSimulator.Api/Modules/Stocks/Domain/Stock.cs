using SharedKernel;

namespace StockMarketSimulator.Api.Modules.Stocks.Domain;

internal sealed class Stock : IEntity
{
    private Stock(Guid id, string ticker, decimal price, DateTime timestamp)
    {
        Ensure.NotNullOrEmpty(id, nameof(id));
        Ensure.NotNullOrEmpty(ticker, nameof(ticker));
        Ensure.GreaterThanOrEqualToZero(price, nameof(price));
        Ensure.NotNull(timestamp, nameof(timestamp));

        Id = id;
        Ticker = ticker;
        Price = price;
        Timestamp = timestamp;
    }

    private Stock()
    {
    }

    public Guid Id { get; private set; }

    public string Ticker { get; private set; }

    public decimal Price { get; private set; }

    public DateTime Timestamp { get; private set; }

    public static Stock Create(string ticker, decimal price)
    {
        return new Stock(Guid.NewGuid(), ticker, price, DateTime.UtcNow);
    }

    public void UpdatePrice(decimal newPrice)
    {
        Ensure.GreaterThanOrEqualToZero(newPrice, nameof(newPrice));

        Price = newPrice;
        Timestamp = DateTime.UtcNow;
    }
}
