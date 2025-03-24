using Npgsql;

namespace StockMarketSimulator.Api.Modules.Transactions.Domain;

internal interface ITransactionRepository
{
    Task<Transaction?> GetByIdAsync(
        NpgsqlConnection connection, 
        Guid transactionId, 
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default);

    Task<List<Transaction>> GetByUserIdAsync(
        NpgsqlConnection connection,
        Guid userId,
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default);

    Task CreateAsync(
        NpgsqlConnection connection,
        Transaction transactionModel,
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default);

    Task DeleteAsync(
        NpgsqlConnection connection,
        Guid transactionId,
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default);
}
