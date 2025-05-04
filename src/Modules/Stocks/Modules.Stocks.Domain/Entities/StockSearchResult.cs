using Modules.Stocks.Domain.DomainEvents;
using SharedKernel;

namespace Modules.Stocks.Domain.Entities;

public sealed class StockSearchResult : Entity, IAuditable
{
    private StockSearchResult(
        Guid id,
        string ticker, 
        string name, 
        string type, 
        string region, 
        string marketOpen, 
        string marketClose, 
        string timezone, 
        string currency)
    {
        Ensure.NotNullOrEmpty(ticker, nameof(ticker));
        Ensure.NotNullOrEmpty(name, nameof(name));
        Ensure.NotNullOrEmpty(type, nameof(type));
        Ensure.NotNullOrEmpty(region, nameof(region));
        Ensure.NotNullOrEmpty(marketOpen, nameof(marketOpen));
        Ensure.NotNullOrEmpty(marketClose, nameof(marketClose));
        Ensure.NotNullOrEmpty(timezone, nameof(timezone));
        Ensure.NotNullOrEmpty(currency, nameof(currency));
        Ensure.NotNullOrEmpty(id, nameof(id));

        Id = id;
        Ticker = ticker;
        Name = name;
        Type = type;
        Region = region;
        MarketOpen = marketOpen;
        MarketClose = marketClose;
        Timezone = timezone;
        Currency = currency;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="StockSearchResult"/>.
    /// </summary>
    /// <remarks>
    /// Required for EF Core.
    /// </remarks>
    private StockSearchResult()
    {
    }

    public Guid Id { get; private set; }

    public string Ticker { get; private set; }

    public string Name { get; private set; }

    public string Type { get; private set; }

    public string Region { get; private set; }

    public string MarketOpen { get; private set; }

    public string MarketClose { get; private set; }

    public string Timezone { get; private set; }

    public string Currency { get; private set; }

    public DateTime CreatedOnUtc { get; set; }

    public DateTime? ModifiedOnUtc { get; set; }

    public static StockSearchResult Create(
        string ticker,
        string name,
        string type,
        string region,
        string marketOpen,
        string marketClose,
        string timezone,
        string currency)
    {
        var stockResult = new StockSearchResult(
            Guid.CreateVersion7(),
            ticker,
            name,
            type,
            region,
            marketOpen,
            marketClose,
            timezone,
            currency);

        stockResult.Raise(new StockResultCreatedDomainEvent(Guid.CreateVersion7(), stockResult.Id));

        return stockResult;
    }
}
