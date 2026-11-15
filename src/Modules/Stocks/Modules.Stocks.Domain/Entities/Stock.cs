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

    public string Ticker { get; private set; } = null!;

    public decimal Price { get; private set; }

    /// <summary>Company or coin name for built-in listings; <c>null</c> for symbols fetched on demand.</summary>
    public string? Name { get; private set; }

    public string? Sector { get; private set; }

    /// <summary><c>stock</c> or <c>crypto</c> (see <see cref="Services.AssetClasses"/>).</summary>
    public string AssetClass { get; private set; } = Services.AssetClasses.Stock;

    /// <summary>The simulated trading day (UTC date) the session statistics below belong to.</summary>
    public DateOnly SessionDate { get; private set; }

    public decimal OpenPrice { get; private set; }

    /// <summary>Last price of the previous session; the baseline for the day change.</summary>
    public decimal PreviousClose { get; private set; }

    public decimal DayHigh { get; private set; }

    public decimal DayLow { get; private set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? ModifiedOnUtc { get; set; }

    public static Stock Create(string ticker, decimal price) => Create(ticker, price, DateOnly.FromDateTime(DateTime.UtcNow));

    public static Stock Create(
        string ticker,
        decimal price,
        DateOnly sessionDate,
        string? name = null,
        string? sector = null,
        string assetClass = Services.AssetClasses.Stock)
    {
        var stock = new Stock(Guid.CreateVersion7(), ticker, price)
        {
            Name = name,
            Sector = sector,
            AssetClass = assetClass,
            SessionDate = sessionDate,
            OpenPrice = price,
            PreviousClose = price,
            DayHigh = price,
            DayLow = price,
        };

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
