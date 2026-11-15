using System.Data;
using Application.Abstractions.Data;
using Dapper;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.Stocks.Application.Abstractions.Realtime;
using Modules.Stocks.Contracts.Stocks;
using Modules.Stocks.Domain.Services;
using Modules.Stocks.Infrastructure.Realtime.Options;
using Quartz;
using SharedKernel;

namespace Modules.Stocks.Infrastructure.Realtime;

/// <summary>
/// Advances every live ticker by one simulation step and pushes the ticks to SignalR clients.
/// </summary>
/// <remarks>
/// <para>
/// Live tickers are the built-in market universe (always) plus any other symbol someone is watching.
/// Each step:
/// </para>
/// <list type="number">
///   <item>draws one shared <em>market factor</em> and mixes it into every asset by its beta, so the market
///   moves together;</item>
///   <item>applies a GBM step (clamped per tick) and, rarely, a news shock of a few percent;</item>
///   <item>rolls the session at UTC midnight (previous close, open, high and low);</item>
///   <item>persists all prices in one statement and upserts the current one-minute bars;</item>
///   <item>broadcasts the ticks and any news.</item>
/// </list>
/// Persisting every step means the next tick continues from the new price and trades fill at exactly
/// the price users see.
/// </remarks>
[DisallowConcurrentExecution]
public sealed class StocksFeedUpdater(
    IActiveTickerManager activeTickerManager,
    IServiceScopeFactory serviceScopeFactory,
    IHubContext<StocksFeedHub, IStocksUpdateClient> hubContext,
    IOptions<StockUpdateOptions> options,
    IMarketNewsStore newsStore,
    IDateTimeProvider dateTimeProvider,
    ILogger<StocksFeedUpdater> logger,
    IStockVolatilityProvider stockVolatilityProvider) : IJob
{
    public const string Name = nameof(StocksFeedUpdater);

    /// <summary>Beta for symbols outside the built-in universe.</summary>
    private const double DefaultBeta = 0.6;

    private readonly StockUpdateOptions _options = options.Value;

    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        string[] tickers = [.. activeTickerManager.GetAllTickers()
            .Union(MarketCatalog.Tickers, StringComparer.OrdinalIgnoreCase)
            .Select(t => t.ToUpperInvariant())
            .Distinct()];

        if (tickers.Length == 0)
        {
            return;
        }

        try
        {
            await TickAsync(tickers, cancellationToken);
        }
        catch (Exception exception) when (exception is not OperationCanceledException)
        {
            // The next tick tries again; a failing tick must not kill the trigger.
            logger.LogError(exception, "Simulated price tick failed");
        }
    }

    private async Task TickAsync(string[] tickers, CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        IDbConnectionFactory connectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();

        using IDbConnection connection = await connectionFactory.GetOpenConnectionAsync(cancellationToken);

        List<StockRow> rows = [.. await connection.QueryAsync<StockRow>(new CommandDefinition(
            """
            SELECT ticker AS Ticker, price AS Price, open_price AS OpenPrice, previous_close AS PreviousClose,
                   day_high AS DayHigh, day_low AS DayLow, session_date AS SessionDate
            FROM stocks.stocks
            WHERE ticker = ANY(@Tickers);
            """,
            new { Tickers = tickers },
            cancellationToken: cancellationToken))];

        if (rows.Count == 0)
        {
            return;
        }

        DateTime now = dateTimeProvider.UtcNow;
        DateTime today = now.Date;
        double step = _options.UpdateIntervalInSeconds / GeometricBrownianMotion.TradingSecondsPerDay;
        double marketFactor = GeometricBrownianMotion.SampleStandardNormal(Random.Shared);

        // Expected number of news events per tick across the whole market.
        double newsProbability = _options.NewsEventsPerMinute * _options.UpdateIntervalInSeconds / 60.0 / rows.Count;

        var updates = new List<StockPriceUpdate>(rows.Count);
        var news = new List<MarketNewsResponse>();

        foreach (StockRow row in rows)
        {
            if (row.Price <= 0)
            {
                continue;
            }

            (double mu, double sigma) = stockVolatilityProvider.GetParameters(row.Ticker);
            bool listed = MarketCatalog.TryGet(row.Ticker, out ListedAsset asset);
            double beta = listed ? asset.Beta : DefaultBeta;

            // Correlated shock: beta of it is the market, the rest is the asset's own noise.
            double shock = (beta * marketFactor) +
                (Math.Sqrt(1 - (beta * beta)) * GeometricBrownianMotion.SampleStandardNormal(Random.Shared));

            decimal price = GeometricBrownianMotion.NextPrice(
                row.Price, mu, sigma, step, _options.MaxPercentageChange, shock);

            if (listed && Random.Shared.NextDouble() < newsProbability)
            {
                MarketNewsEvent newsEvent = MarketNewsGenerator.Create(asset, Random.Shared);
                price = PriceHistoryGenerator.ToPrice((double)price * (1 + newsEvent.ImpactPercent));

                news.Add(new MarketNewsResponse(
                    Guid.CreateVersion7(),
                    newsEvent.Ticker,
                    newsEvent.Headline,
                    Math.Round((decimal)newsEvent.ImpactPercent, 4),
                    newsEvent.ImpactPercent >= 0 ? "positive" : "negative",
                    now));
            }

            // A new UTC day starts a new session: yesterday's last price becomes the previous close.
            bool newSession = row.SessionDate.Date < today || row.PreviousClose <= 0;
            decimal previousClose = newSession ? row.Price : row.PreviousClose;
            decimal open = newSession ? price : row.OpenPrice;
            decimal high = newSession ? price : Math.Max(row.DayHigh, price);
            decimal low = newSession ? price : Math.Min(row.DayLow <= 0 ? price : row.DayLow, price);

            updates.Add(new StockPriceUpdate(row.Ticker, price, open, previousClose, high, low, now));
        }

        await PersistAsync(connection, updates, now, cancellationToken);

        foreach (StockPriceUpdate update in updates)
        {
            await hubContext.Clients.Group(update.Ticker).ReceiveStockPriceUpdate(update, cancellationToken);
        }

        foreach (MarketNewsResponse item in news)
        {
            newsStore.Add(item);
            await hubContext.Clients.All.ReceiveMarketNews(item, cancellationToken);

            logger.LogInformation("Market news: {Headline} ({Impact:P1})", item.Headline, item.ImpactPercent);
        }
    }

    private static async Task PersistAsync(
        IDbConnection connection,
        List<StockPriceUpdate> updates,
        DateTime now,
        CancellationToken cancellationToken)
    {
        if (updates.Count == 0)
        {
            return;
        }

        var bucket = new DateTime(now.Ticks - (now.Ticks % TimeSpan.TicksPerMinute), DateTimeKind.Utc);

        var parameters = new
        {
            Tickers = updates.Select(u => u.Ticker).ToArray(),
            Prices = updates.Select(u => u.Price).ToArray(),
            Opens = updates.Select(u => u.Open).ToArray(),
            PreviousCloses = updates.Select(u => u.PreviousClose).ToArray(),
            Highs = updates.Select(u => u.DayHigh).ToArray(),
            Lows = updates.Select(u => u.DayLow).ToArray(),
            Today = DateTime.SpecifyKind(now.Date, DateTimeKind.Unspecified),
            Now = now,
            Bucket = bucket,
        };

        const string sql =
            """
            UPDATE stocks.stocks AS s
            SET price = u.price,
                open_price = u.open,
                previous_close = u.previous_close,
                day_high = u.high,
                day_low = u.low,
                session_date = @Today::date,
                modified_on_utc = @Now
            FROM unnest(@Tickers, @Prices, @Opens, @PreviousCloses, @Highs, @Lows)
                AS u(ticker, price, open, previous_close, high, low)
            WHERE s.ticker = u.ticker;

            INSERT INTO stocks.price_bars (ticker, bucket_start, open, high, low, close)
            SELECT u.ticker, @Bucket, u.price, u.price, u.price, u.price
            FROM unnest(@Tickers, @Prices) AS u(ticker, price)
            ON CONFLICT (ticker, bucket_start) DO UPDATE
            SET high = GREATEST(price_bars.high, EXCLUDED.close),
                low = LEAST(price_bars.low, EXCLUDED.close),
                close = EXCLUDED.close;
            """;

        await connection.ExecuteAsync(new CommandDefinition(sql, parameters, cancellationToken: cancellationToken));
    }

    private sealed class StockRow
    {
        public string Ticker { get; init; } = string.Empty;

        public decimal Price { get; init; }

        public decimal OpenPrice { get; init; }

        public decimal PreviousClose { get; init; }

        public decimal DayHigh { get; init; }

        public decimal DayLow { get; init; }

        public DateTime SessionDate { get; init; }
    }
}
