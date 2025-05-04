using Modules.Stocks.Domain.DomainEvents;
using SharedKernel;

namespace Modules.Stocks.Domain.Entities;

public sealed class Stock : Entity, IAuditable
{
    private Stock(Guid id, string ticker, decimal price)
    {
        Ensure.NotNullOrEmpty(id, nameof(id));
        Ensure.NotNullOrEmpty(ticker, nameof(ticker));
        Ensure.GreaterThanOrEqualToZero(price, nameof(price));

        Id = id;
        Ticker = ticker;
        Price = price;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Stock"/>.
    /// </summary>
    /// <remarks>
    /// Required for EF Core.
    /// </remarks>
    private Stock()
    {
    }

    public Guid Id { get; private set; }

    public string Ticker { get; private set; }

    public decimal Price { get; private set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? ModifiedOnUtc { get; set; }

    public static Stock Create(string ticker, decimal price)
    {
        var stock = new Stock(Guid.CreateVersion7(), ticker, price);

        stock.Raise(new StockCreatedDomainEvent(Guid.CreateVersion7(), stock.Id));

        return stock;
    }

    public void ChangePrice(decimal newPrice)
    {
        Ensure.GreaterThanZero(newPrice, nameof(newPrice));
        if (Price == newPrice)
        {
            return;
        }

        Price = newPrice;
    }
}
