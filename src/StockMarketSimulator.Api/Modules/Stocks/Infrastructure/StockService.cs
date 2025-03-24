using Dapper;
using StockMarketSimulator.Api.Infrastructure.Database;
using StockMarketSimulator.Api.Modules.Stocks.Contracts;
using StockMarketSimulator.Api.Modules.Stocks.Domain;

namespace StockMarketSimulator.Api.Modules.Stocks.Infrastructure;

internal sealed class StockService : IStockService
{
    private readonly ActiveTickerManager _activeTickerManager;
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IStocksClient _stocksClient;
    private readonly ILogger<StockService> _logger;

    public StockService(
        ActiveTickerManager activeTickerManager,
        IDbConnectionFactory dbConnectionFactory,
        IStocksClient stocksClient,
        ILogger<StockService> logger)
    {
        _activeTickerManager = activeTickerManager;
        _dbConnectionFactory = dbConnectionFactory;
        _stocksClient = stocksClient;
        _logger = logger;
    }

    public async Task<StockPriceResponse?> GetLatestStockPriceAsync(
        string ticker, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            // First, try to get the latest price from the database
            StockPriceResponse? dbPrice = await GetLatestPriceFromDatabaseAsync(ticker, cancellationToken);
            if (dbPrice is not null)
            {
                _activeTickerManager.AddTicker(ticker);

                return dbPrice;
            }

            // If not found in the database, fetch from the external API
            StockPriceResponse? apiPrice = await _stocksClient.GetDataForTickerAsync(ticker, cancellationToken);
            if (apiPrice is null)
            {
                _logger.LogWarning("No data returned from external API for ticker {Ticker}", ticker);
                return null;
            }

            // Save the new price to the database
            await SavePriceToDatabaseAsync(apiPrice, cancellationToken);

            _activeTickerManager.AddTicker(ticker);

            return apiPrice;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Error occured while fetching stock price for ticker: {Ticker}", ticker);
            throw;
        }
    }

    private async Task<StockPriceResponse?> GetLatestPriceFromDatabaseAsync(
        string ticker,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT 
                ticker AS Ticker,
                price AS Price,
                timestamp AS Timestamp
            FROM public.stock_prices
            WHERE ticker = @Ticker
            ORDER BY timestamp DESC
            LIMIT 1;
            """;

        await using var connection = await _dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

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

    private async Task SavePriceToDatabaseAsync(StockPriceResponse price, CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            INSERT INTO public.stock_prices (id, ticker, price, timestamp)
            VALUES (@Id, @Ticker, @Price, @Timestamp);
            """;

        await using var connection = await _dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        await connection.ExecuteAsync(
            sql,
            new
            {
                Id = Guid.NewGuid(),
                Ticker = price.Ticker,
                Price = price.Price,
                Timestamp = DateTime.UtcNow,
            });
    }
}
