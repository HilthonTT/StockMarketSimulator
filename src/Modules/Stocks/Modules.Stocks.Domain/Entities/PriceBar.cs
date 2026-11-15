namespace Modules.Stocks.Domain.Entities;

/// <summary>
/// One OHLC bar of a ticker's price history. Live bars are one minute long; back-filled history
/// uses coarser bars. Queries re-bucket them to the requested resolution.
/// </summary>
public sealed class PriceBar
{
    public PriceBar(string ticker, DateTime bucketStart, decimal open, decimal high, decimal low, decimal close)
    {
        Ticker = ticker;
        BucketStart = bucketStart;
        Open = open;
        High = high;
        Low = low;
        Close = close;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PriceBar"/>.
    /// </summary>
    /// <remarks>
    /// Required for EF Core.
    /// </remarks>
    private PriceBar()
    {
    }

    public string Ticker { get; private set; } = null!;

    public DateTime BucketStart { get; private set; }

    public decimal Open { get; private set; }

    public decimal High { get; private set; }

    public decimal Low { get; private set; }

    public decimal Close { get; private set; }
}
