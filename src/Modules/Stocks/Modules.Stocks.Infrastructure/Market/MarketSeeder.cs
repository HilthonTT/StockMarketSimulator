using System.Data;
using Application.Abstractions.Data;
using Dapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Modules.Stocks.Domain.Services;
using SharedKernel;

namespace Modules.Stocks.Infrastructure.Market;

/// <summary>
/// Lists the built-in market universe on start-up: creates the stock rows (with session statistics),
/// makes them searchable, and back-fills price history for any listing without one. Idempotent.
/// </summary>
internal sealed class MarketSeeder(
    IServiceScopeFactory serviceScopeFactory,
    IDateTimeProvider dateTimeProvider,
    ILogger<MarketSeeder> logger) : BackgroundService
{
    private static readonly TimeSpan RetryDelay = TimeSpan.FromSeconds(10);
    private const int MaxAttempts = 30;

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // The schema may not exist yet: migrations run at start-up in Development and by hand elsewhere.
        for (int attempt = 1; attempt <= MaxAttempts && !stoppingToken.IsCancellationRequested; attempt++)
        {
            try
            {
                await SeedAsync(stoppingToken);
                return;
            }
            catch (Exception exception) when (exception is not OperationCanceledException)
            {
                logger.LogWarning(exception, "Seeding the market universe failed (attempt {Attempt}). Retrying.", attempt);

                await Task.Delay(RetryDelay, stoppingToken);
            }
        }
    }

    private async Task SeedAsync(CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        IDbConnectionFactory connectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();

        using IDbConnection connection = await connectionFactory.GetOpenConnectionAsync(cancellationToken);

        DateTime now = dateTimeProvider.UtcNow;
        var today = DateOnly.FromDateTime(now);

        foreach (ListedAsset asset in MarketCatalog.Assets)
        {
            await UpsertStockAsync(connection, asset, now, today, cancellationToken);
            await InsertSearchResultAsync(connection, asset, now, cancellationToken);

            bool hasHistory = await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
                "SELECT EXISTS (SELECT 1 FROM stocks.price_bars WHERE ticker = @Ticker);",
                new { asset.Ticker },
                cancellationToken: cancellationToken));

            if (hasHistory)
            {
                continue;
            }

            decimal price = await connection.ExecuteScalarAsync<decimal>(new CommandDefinition(
                "SELECT price FROM stocks.stocks WHERE ticker = @Ticker;",
                new { asset.Ticker },
                cancellationToken: cancellationToken));

            await PriceHistorySeeder.SeedAsync(
                connection, asset.Ticker, price, asset.DailyDrift, asset.DailyVolatility, now, cancellationToken);

            await ApplySessionFromHistoryAsync(connection, asset.Ticker, now, cancellationToken);
        }

        logger.LogInformation("Market universe ready: {Count} listings", MarketCatalog.Assets.Count);
    }

    /// <summary>
    /// Derives today's session from the back-filled history, so a fresh install already shows real
    /// day changes, gainers and losers instead of every listing at 0%.
    /// </summary>
    private static Task<int> ApplySessionFromHistoryAsync(
        IDbConnection connection,
        string ticker,
        DateTime now,
        CancellationToken cancellationToken)
    {
        const string sql =
            """
            WITH today AS (
                SELECT (array_agg(open ORDER BY bucket_start))[1] AS open, MAX(high) AS high, MIN(low) AS low
                FROM stocks.price_bars
                WHERE ticker = @Ticker AND bucket_start >= @Midnight
            ),
            previous AS (
                SELECT close FROM stocks.price_bars
                WHERE ticker = @Ticker AND bucket_start < @Midnight
                ORDER BY bucket_start DESC
                LIMIT 1
            )
            UPDATE stocks.stocks AS s
            SET open_price = COALESCE(today.open, s.price),
                day_high = GREATEST(COALESCE(today.high, s.price), s.price),
                day_low = LEAST(COALESCE(today.low, s.price), s.price),
                previous_close = COALESCE((SELECT close FROM previous), s.price)
            FROM today
            WHERE s.ticker = @Ticker;
            """;

        return connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { Ticker = ticker, Midnight = DateTime.SpecifyKind(now.Date, DateTimeKind.Utc) },
            cancellationToken: cancellationToken));
    }

    private static Task<int> UpsertStockAsync(
        IDbConnection connection,
        ListedAsset asset,
        DateTime now,
        DateOnly today,
        CancellationToken cancellationToken)
    {
        // Existing rows keep their simulated price; only the listing metadata is refreshed.
        const string sql =
            """
            INSERT INTO stocks.stocks
                (id, ticker, price, name, sector, asset_class, session_date,
                 open_price, previous_close, day_high, day_low, created_on_utc)
            VALUES
                (@Id, @Ticker, @Price, @Name, @Sector, @AssetClass, @Today::date,
                 @Price, @Price, @Price, @Price, @Now)
            ON CONFLICT (ticker) DO UPDATE
            SET name = EXCLUDED.name,
                sector = EXCLUDED.sector,
                asset_class = EXCLUDED.asset_class;
            """;

        return connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                Id = Guid.CreateVersion7(),
                asset.Ticker,
                Price = asset.SeedPrice,
                asset.Name,
                asset.Sector,
                asset.AssetClass,
                Today = today.ToDateTime(TimeOnly.MinValue, DateTimeKind.Unspecified),
                Now = now,
            },
            cancellationToken: cancellationToken));
    }

    private static Task<int> InsertSearchResultAsync(
        IDbConnection connection,
        ListedAsset asset,
        DateTime now,
        CancellationToken cancellationToken)
    {
        bool crypto = asset.AssetClass == AssetClasses.Crypto;

        const string sql =
            """
            INSERT INTO stocks.stock_search_results
                (id, ticker, name, type, region, market_open, market_close, timezone, currency, created_on_utc)
            SELECT @Id, @Ticker, @Name, @Type, @Region, @MarketOpen, @MarketClose, @Timezone, @Currency, @Now
            WHERE NOT EXISTS (SELECT 1 FROM stocks.stock_search_results WHERE ticker = @Ticker);
            """;

        return connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                Id = Guid.CreateVersion7(),
                asset.Ticker,
                asset.Name,
                Type = crypto ? "Cryptocurrency" : "Equity",
                Region = crypto ? "Global" : "United States",
                MarketOpen = crypto ? "00:00" : "09:30",
                MarketClose = crypto ? "23:59" : "16:00",
                Timezone = crypto ? "UTC" : "UTC-04",
                Currency = "USD",
                Now = now,
            },
            cancellationToken: cancellationToken));
    }
}
