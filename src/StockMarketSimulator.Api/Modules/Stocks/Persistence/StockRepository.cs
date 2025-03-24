using Dapper;
using Npgsql;
using StockMarketSimulator.Api.Modules.Stocks.Domain;

namespace StockMarketSimulator.Api.Modules.Stocks.Persistence;

internal sealed class StockRepository : IStockRepository
{
    public async Task CreateAsync(
        NpgsqlConnection connection, 
        Stock stock, 
        NpgsqlTransaction? transaction = null, 
        CancellationToken cancellationToken = default)
    {
        const string sql =
           """
            INSERT INTO public.stock_prices (ticker, price, timestamp)
            VALUES (@Ticker, @Price, @Timestamp);
            """;

        await connection.ExecuteAsync(
           sql,
           new
           {
               Ticker = stock.Ticker,
               Price = stock.Price,
               Timestamp = DateTime.UtcNow,
           });
    }

    public Task<Stock?> GetLatestAsync(
        NpgsqlConnection connection, 
        string ticker, 
        NpgsqlTransaction? transaction = null,
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

        return connection.QueryFirstOrDefaultAsync<Stock>(
            sql,
            new
            {
                Ticker = ticker,
            });
    }
}
