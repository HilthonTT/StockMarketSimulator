using System.Data;
using Dapper;
using Modules.Stocks.Contracts.Stocks;

namespace Modules.Stocks.Infrastructure.Market;

internal static class StockQuoteQueries
{
    private const string SelectColumns =
        """
        SELECT
            ticker AS Ticker,
            name AS Name,
            sector AS Sector,
            asset_class AS AssetClass,
            price AS Price,
            open_price AS OpenPrice,
            previous_close AS PreviousClose,
            day_high AS DayHigh,
            day_low AS DayLow,
            COALESCE(modified_on_utc, created_on_utc) AS UpdatedOnUtc
        FROM stocks.stocks
        """;

    public static async Task<StockQuoteResponse?> GetAsync(
        IDbConnection connection,
        string ticker,
        CancellationToken cancellationToken = default)
    {
        QuoteRow? row = await connection.QueryFirstOrDefaultAsync<QuoteRow>(new CommandDefinition(
            $"{SelectColumns} WHERE ticker = @Ticker;",
            new { Ticker = ticker },
            cancellationToken: cancellationToken));

        return row?.ToResponse();
    }

    public static async Task<List<StockQuoteResponse>> GetManyAsync(
        IDbConnection connection,
        IReadOnlyCollection<string> tickers,
        CancellationToken cancellationToken = default)
    {
        IEnumerable<QuoteRow> rows = await connection.QueryAsync<QuoteRow>(new CommandDefinition(
            $"{SelectColumns} WHERE ticker = ANY(@Tickers) ORDER BY ticker;",
            new { Tickers = tickers.ToArray() },
            cancellationToken: cancellationToken));

        return [.. rows.Select(row => row.ToResponse())];
    }

    private sealed class QuoteRow
    {
        public string Ticker { get; init; } = string.Empty;

        public string? Name { get; init; }

        public string? Sector { get; init; }

        public string AssetClass { get; init; } = "stock";

        public decimal Price { get; init; }

        public decimal OpenPrice { get; init; }

        public decimal PreviousClose { get; init; }

        public decimal DayHigh { get; init; }

        public decimal DayLow { get; init; }

        public DateTime UpdatedOnUtc { get; init; }

        public StockQuoteResponse ToResponse()
        {
            // Rows listed before session statistics existed have zeros until their first tick.
            decimal previousClose = PreviousClose > 0 ? PreviousClose : Price;

            return StockQuoteResponse.From(
                Ticker,
                Name,
                Sector,
                AssetClass,
                Price,
                OpenPrice > 0 ? OpenPrice : Price,
                previousClose,
                DayHigh > 0 ? DayHigh : Price,
                DayLow > 0 ? DayLow : Price,
                DateTime.SpecifyKind(UpdatedOnUtc, DateTimeKind.Utc));
        }
    }
}
