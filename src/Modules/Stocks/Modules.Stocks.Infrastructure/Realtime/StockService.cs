using System.Data;
using Application.Abstractions.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Modules.Stocks.Application.Abstractions.Http;
using Modules.Stocks.Application.Abstractions.Realtime;
using Modules.Stocks.Contracts.Stocks;
using Modules.Stocks.Domain.Entities;
using SharedKernel;

namespace Modules.Stocks.Infrastructure.Realtime;

internal sealed class StockService(
    IActiveTickerManager activeTickerManager,
    IDbConnectionFactory dbConnectionFactory,
    IStocksClient stocksClient,
    ILogger<StockService> logger,
    IDateTimeProvider dateTimeProvider) : IStockService
{
    public async Task<Option<StockPriceResponse>> GetLatestStockPriceAsync(
        string ticker, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            using IDbConnection connection = await dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

            // First, try to get the latest price from the database
            StockPriceResponse? dbPrice = await GetLatestPriceFromDatabaseAsync(connection, ticker);
            if (dbPrice is not null)
            {
                activeTickerManager.AddTicker(ticker);

                return Option<StockPriceResponse>.Some(dbPrice);
            }

            // If not found in the database, fetch from the external API
            StockPriceResponse? apiPrice = await stocksClient.GetDataForTickerAsync(ticker, cancellationToken);
            if (apiPrice is null)
            {
                logger.LogWarning("No data returned from external API for ticker {Ticker}", ticker);
                return Option<StockPriceResponse>.None();
            }

            // Save the new price to the database
            await SavePriceToDatabaseAsync(connection, apiPrice);

            activeTickerManager.AddTicker(ticker);

            return Option<StockPriceResponse>.Some(dbPrice);
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "Error occured while fetching stock price for ticker: {Ticker}", ticker);
            throw;
        }
    }

    private static async Task<StockPriceResponse?> GetLatestPriceFromDatabaseAsync(IDbConnection connection, string ticker)
    {
        const string sql =
            """
            SELECT 
                ticker AS Ticker,
                price AS Price,
                created_on_utc AS CreatedOnUtc,
                modified_on_utc AS ModifiedOnUtc
            FROM stocks.stocks
            WHERE ticker = @Ticker
            ORDER BY created_on_utc DESC
            LIMIT 1;
            """;

        Stock? result = await connection.QueryFirstOrDefaultAsync<Stock>(
            sql,
            new
            {
                Ticker = ticker,
            });

        if (result is not null)
        {
            return new StockPriceResponse(result.Ticker, result.Price);
        }

        return null;
    }

    private Task<int> SavePriceToDatabaseAsync(IDbConnection connection, StockPriceResponse price)
    {
        const string sql =
            """
            INSERT INTO stocks.stocks (id, ticker, price, created_on_utc)
            VALUES (@Id, @Ticker, @Price, @CreatedOnUtc);
            """;

        return connection.ExecuteAsync(
            sql,
            new
            {
                Id = Guid.CreateVersion7(),
                Ticker = price.Ticker,
                Price = price.Price,
                CreatedOnUtc = dateTimeProvider.UtcNow,
            });
    }
}
