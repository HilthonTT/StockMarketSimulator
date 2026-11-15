using System.Data;
using Application.Abstractions.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Modules.Stocks.Application.Abstractions.Http;
using Modules.Stocks.Application.Abstractions.Realtime;
using Modules.Stocks.Contracts.Stocks;
using Modules.Stocks.Infrastructure.Market;
using SharedKernel;

namespace Modules.Stocks.Infrastructure.Realtime;

internal sealed class StockService(
    IActiveTickerManager activeTickerManager,
    IDbConnectionFactory dbConnectionFactory,
    IStocksClient stocksClient,
    IStockVolatilityProvider volatilityProvider,
    ILogger<StockService> logger,
    IDateTimeProvider dateTimeProvider) : IStockService
{
    public Task<Option<StockPriceResponse>> GetLatestStockPriceAsync(
        string ticker,
        CancellationToken cancellationToken = default) =>
        GetLatestStockPriceAsync(ticker, trackActivity: true, cancellationToken);

    public async Task<Option<StockPriceResponse>> GetLatestStockPriceAsync(
        string ticker,
        bool trackActivity,
        CancellationToken cancellationToken = default)
    {
        Option<StockQuoteResponse> quote = await GetQuoteAsync(ticker, trackActivity, cancellationToken);

        return quote.IsSome
            ? Option<StockPriceResponse>.Some(new StockPriceResponse(quote.ValueOrThrow().Ticker, quote.ValueOrThrow().Price))
            : Option<StockPriceResponse>.None();
    }

    public async Task<Option<StockQuoteResponse>> GetQuoteAsync(
        string ticker,
        bool trackActivity = true,
        CancellationToken cancellationToken = default)
    {
        // Tickers are stored and broadcast upper-case; "aapl" and "AAPL" must be the same stock.
        ticker = ticker.Trim().ToUpperInvariant();

        using IDbConnection connection = await dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        StockQuoteResponse? quote = await StockQuoteQueries.GetAsync(connection, ticker, cancellationToken);

        if (quote is null)
        {
            // Not listed yet: fetch a real quote (when an API key is configured) and list it.
            StockPriceResponse? apiPrice = await stocksClient.GetDataForTickerAsync(ticker, cancellationToken);
            if (apiPrice is null || apiPrice.Price <= 0)
            {
                logger.LogInformation("No quote available for ticker {Ticker}", ticker);
                return Option<StockQuoteResponse>.None();
            }

            await ListAsync(connection, ticker, apiPrice.Price, cancellationToken);

            quote = await StockQuoteQueries.GetAsync(connection, ticker, cancellationToken);
            if (quote is null)
            {
                return Option<StockQuoteResponse>.None();
            }
        }

        if (trackActivity)
        {
            activeTickerManager.AddTicker(ticker);
        }

        return Option<StockQuoteResponse>.Some(quote);
    }

    public async Task UpdatePriceAsync(string ticker, decimal price, CancellationToken cancellationToken = default)
    {
        using IDbConnection connection = await dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        const string sql =
            """
            UPDATE stocks.stocks
            SET price = @Price,
                day_high = GREATEST(day_high, @Price),
                day_low = LEAST(day_low, @Price),
                modified_on_utc = @UtcNow
            WHERE ticker = @Ticker;
            """;

        await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new { Ticker = ticker.Trim().ToUpperInvariant(), Price = price, dateTimeProvider.UtcNow },
            cancellationToken: cancellationToken));
    }

    private async Task ListAsync(IDbConnection connection, string ticker, decimal price, CancellationToken cancellationToken)
    {
        DateTime now = dateTimeProvider.UtcNow;

        const string sql =
            """
            INSERT INTO stocks.stocks
                (id, ticker, price, asset_class, session_date, open_price, previous_close, day_high, day_low, created_on_utc)
            VALUES
                (@Id, @Ticker, @Price, 'stock', @Today::date, @Price, @Price, @Price, @Price, @Now)
            ON CONFLICT (ticker) DO NOTHING;
            """;

        int inserted = await connection.ExecuteAsync(new CommandDefinition(
            sql,
            new
            {
                Id = Guid.CreateVersion7(),
                Ticker = ticker,
                Price = price,
                Today = DateTime.SpecifyKind(now.Date, DateTimeKind.Unspecified),
                Now = now,
            },
            cancellationToken: cancellationToken));

        // Only the request that actually listed the ticker back-fills its history.
        if (inserted > 0)
        {
            (double mu, double sigma) = volatilityProvider.GetParameters(ticker);

            await PriceHistorySeeder.SeedAsync(connection, ticker, price, mu, sigma, now, cancellationToken);
        }
    }
}
