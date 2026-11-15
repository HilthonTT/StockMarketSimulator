using System.Data;
using Application.Abstractions.Data;
using Dapper;
using Modules.Stocks.Application.Abstractions.Realtime;
using Modules.Stocks.Contracts.Stocks;
using Modules.Stocks.Domain.Services;
using SharedKernel;

namespace Modules.Stocks.Infrastructure.Market;

internal sealed class MarketDataService(
    IDbConnectionFactory dbConnectionFactory,
    IDateTimeProvider dateTimeProvider) : IMarketDataService
{
    public async Task<MarketOverviewResponse> GetOverviewAsync(CancellationToken cancellationToken = default)
    {
        using IDbConnection connection = await dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        List<StockQuoteResponse> quotes = await StockQuoteQueries.GetManyAsync(
            connection, [.. MarketCatalog.Tickers], cancellationToken);

        // Price-weighted, like the Dow: the sum of stock prices over a fixed divisor.
        List<StockQuoteResponse> stocks = [.. quotes.Where(q => q.AssetClass == AssetClasses.Stock)];

        decimal value = stocks.Sum(q => q.Price) / MarketCatalog.IndexDivisor;
        decimal previous = stocks.Sum(q => q.PreviousClose) / MarketCatalog.IndexDivisor;
        decimal change = value - previous;

        var index = new MarketIndexResponse(
            MarketCatalog.IndexName,
            Math.Round(value, 2),
            Math.Round(change, 2),
            previous == 0 ? 0 : Math.Round(change / previous, 6));

        Dictionary<string, decimal[]> sparklines = await GetSparklinesAsync(connection, cancellationToken);

        return new MarketOverviewResponse(index, quotes, sparklines);
    }

    private async Task<Dictionary<string, decimal[]>> GetSparklinesAsync(
        IDbConnection connection,
        CancellationToken cancellationToken)
    {
        // One query for the whole universe instead of one history request per table row.
        const string sql =
            """
            SELECT ticker AS Ticker,
                   date_bin(INTERVAL '30 minutes', bucket_start, TIMESTAMPTZ '2000-01-01') AS Time,
                   (array_agg(close ORDER BY bucket_start DESC))[1] AS Close
            FROM stocks.price_bars
            WHERE ticker = ANY(@Tickers) AND bucket_start >= @From
            GROUP BY 1, 2
            ORDER BY 1, 2;
            """;

        IEnumerable<(string Ticker, DateTime Time, decimal Close)> rows =
            await connection.QueryAsync<(string, DateTime, decimal)>(new CommandDefinition(
                sql,
                new { Tickers = MarketCatalog.Tickers.ToArray(), From = dateTimeProvider.UtcNow.AddDays(-1) },
                cancellationToken: cancellationToken));

        return rows
            .GroupBy(r => r.Ticker)
            .ToDictionary(g => g.Key, g => g.Select(r => r.Close).ToArray(), StringComparer.OrdinalIgnoreCase);
    }

    public async Task<PriceHistoryResponse> GetHistoryAsync(
        string ticker,
        PriceRange range,
        CancellationToken cancellationToken = default)
    {
        ticker = ticker.Trim().ToUpperInvariant();

        using IDbConnection connection = await dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        // Re-bucket whatever bars exist (1-minute live bars, coarser back-filled ones) to the range's
        // resolution: first open, max high, min low, last close.
        const string sql =
            """
            SELECT
                date_bin(@Bucket, bucket_start, TIMESTAMPTZ '2000-01-01') AS Time,
                (array_agg(open ORDER BY bucket_start))[1] AS Open,
                MAX(high) AS High,
                MIN(low) AS Low,
                (array_agg(close ORDER BY bucket_start DESC))[1] AS Close
            FROM stocks.price_bars
            WHERE ticker = @Ticker AND bucket_start >= @From
            GROUP BY 1
            ORDER BY 1;
            """;

        IEnumerable<PricePointRow> rows = await connection.QueryAsync<PricePointRow>(new CommandDefinition(
            sql,
            new { Ticker = ticker, range.Bucket, From = dateTimeProvider.UtcNow - range.Span },
            cancellationToken: cancellationToken));

        List<PricePointResponse> points = [.. rows.Select(r => new PricePointResponse(
            DateTime.SpecifyKind(r.Time, DateTimeKind.Utc), r.Open, r.High, r.Low, r.Close))];

        return new PriceHistoryResponse(ticker, range.Name, points);
    }

    public async Task<IReadOnlyDictionary<string, decimal>> GetPricesAsync(
        IReadOnlyCollection<string> tickers,
        CancellationToken cancellationToken = default)
    {
        if (tickers.Count == 0)
        {
            return new Dictionary<string, decimal>();
        }

        using IDbConnection connection = await dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        IEnumerable<(string Ticker, decimal Price)> rows = await connection.QueryAsync<(string, decimal)>(
            new CommandDefinition(
                "SELECT ticker, price FROM stocks.stocks WHERE ticker = ANY(@Tickers);",
                new { Tickers = tickers.Select(t => t.Trim().ToUpperInvariant()).ToArray() },
                cancellationToken: cancellationToken));

        return rows.ToDictionary(r => r.Ticker, r => r.Price, StringComparer.OrdinalIgnoreCase);
    }

    private sealed class PricePointRow
    {
        public DateTime Time { get; init; }

        public decimal Open { get; init; }

        public decimal High { get; init; }

        public decimal Low { get; init; }

        public decimal Close { get; init; }
    }
}
