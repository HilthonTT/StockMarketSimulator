using Npgsql;

namespace StockMarketSimulator.Api.Modules.Stocks.Domain;

internal interface IStockRepository
{
    Task<Stock?> GetLatestAsync(
        NpgsqlConnection connection, 
        string ticker, 
        NpgsqlTransaction? transaction = null, 
        CancellationToken cancellationToken = default);

    Task CreateAsync(
        NpgsqlConnection connection,
        Stock stock,
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default);
}
