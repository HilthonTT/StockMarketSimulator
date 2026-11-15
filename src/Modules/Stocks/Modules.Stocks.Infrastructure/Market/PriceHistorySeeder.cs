using System.Data;
using Dapper;
using Modules.Stocks.Domain.Services;

namespace Modules.Stocks.Infrastructure.Market;

/// <summary>
/// Back-fills a synthetic price history for a ticker that has none, ending at its current price:
/// 89 days of hourly bars plus the last 24 hours in 5-minute bars, so every chart range has data.
/// </summary>
internal static class PriceHistorySeeder
{
    private static readonly TimeSpan HourlyBar = TimeSpan.FromHours(1);
    private static readonly TimeSpan IntradayBar = TimeSpan.FromMinutes(5);

    private const int HourlyBars = 89 * 24;
    private const int IntradayBars = 24 * 12;

    public static async Task SeedAsync(
        IDbConnection connection,
        string ticker,
        decimal currentPrice,
        double dailyDrift,
        double dailyVolatility,
        DateTime nowUtc,
        CancellationToken cancellationToken = default)
    {
        if (currentPrice <= 0)
        {
            return;
        }

        var random = new Random(HashCode.Combine(ticker, nowUtc.Ticks));

        // Start of the current minute; the live feed writes from there on.
        var end = new DateTime(nowUtc.Ticks - (nowUtc.Ticks % TimeSpan.TicksPerMinute), DateTimeKind.Utc);

        IReadOnlyList<PriceCandle> intraday = PriceHistoryGenerator.Generate(
            currentPrice, end, IntradayBar, IntradayBars, dailyDrift, dailyVolatility, random);

        // The hourly history ends where the intraday history starts, at its first open.
        IReadOnlyList<PriceCandle> hourly = PriceHistoryGenerator.Generate(
            intraday[0].Open, intraday[0].Start, HourlyBar, HourlyBars, dailyDrift, dailyVolatility, random);

        PriceCandle[] candles = [.. hourly, .. intraday];

        await InsertAsync(connection, ticker, candles, cancellationToken);
    }

    private static Task<int> InsertAsync(
        IDbConnection connection,
        string ticker,
        IReadOnlyCollection<PriceCandle> candles,
        CancellationToken cancellationToken)
    {
        const string sql =
            """
            INSERT INTO stocks.price_bars (ticker, bucket_start, open, high, low, close)
            SELECT @Ticker, b.bucket_start, b.open, b.high, b.low, b.close
            FROM unnest(@Starts, @Opens, @Highs, @Lows, @Closes) AS b(bucket_start, open, high, low, close)
            ON CONFLICT (ticker, bucket_start) DO NOTHING;
            """;

        return connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                Ticker = ticker,
                Starts = candles.Select(c => c.Start).ToArray(),
                Opens = candles.Select(c => c.Open).ToArray(),
                Highs = candles.Select(c => c.High).ToArray(),
                Lows = candles.Select(c => c.Low).ToArray(),
                Closes = candles.Select(c => c.Close).ToArray(),
            },
            cancellationToken: cancellationToken));
    }
}
