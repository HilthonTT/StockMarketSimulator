using Dapper;
using Npgsql;
using StockMarketSimulator.Api.Modules.Transactions.Domain;

namespace StockMarketSimulator.Api.Modules.Transactions.Persistence;

internal sealed class TransactionRepository : ITransactionRepository
{
    public Task CreateAsync(
        NpgsqlConnection connection, 
        Transaction transactionModel, 
        NpgsqlTransaction? transaction = null, 
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            INSERT INTO public.transactions (id, user_id, ticker, limit_price, transaction_type, quantity, created_on_utc)
            VALUES (@Id, @UserId, @Ticker, @LimitPrice, @TransactionType, @Quantity, @CreatedOnUtc);
            """;

        return connection.ExecuteAsync(sql, new
        {
            Id = transactionModel.Id,
            UserId = transactionModel.UserId,
            Ticker = transactionModel.Ticker,
            LimitPrice = transactionModel.LimitPrice,
            TransactionType = transactionModel.Type,
            Quantity = transactionModel.Quantity,
            CreatedOnUtc = transactionModel.CreatedOnUtc,
        },
        transaction: transaction);
    }

    public Task DeleteAsync(
         NpgsqlConnection connection,
         Guid transactionId,
         NpgsqlTransaction? transaction = null,
         CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            DELETE FROM public.transactions
            WHERE id = @TransactionId;
            """;

        return connection.ExecuteAsync(sql, new
        {
            TransactionId = transactionId,
        },
        transaction: transaction);
    }

    public Task<Transaction?> GetByIdAsync(
        NpgsqlConnection connection,
        Guid transactionId,
        NpgsqlTransaction? transaction = null, 
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT
                id AS Id,
                user_id AS UserId,
                ticker AS Ticker,
                limit_price AS LimitPrice,
                transaction_type AS Type,
                quantity AS Quantity,
                created_on_utc AS CreatedOnUtc
            FROM public.transactions
            WHERE id = @Id;
            """;

        return connection.QueryFirstOrDefaultAsync<Transaction>(
            sql,
            new { Id = transactionId },
            transaction: transaction);
    }

    public async Task<List<Transaction>> GetByUserIdAsync(
        NpgsqlConnection connection,
        Guid userId,
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT
                id AS Id,
                user_id AS UserId,
                ticker AS Ticker,
                limit_price AS LimitPrice,
                transaction_type AS Type,
                quantity AS Quantity,
                created_on_utc AS CreatedOnUtc
            FROM public.transactions
            WHERE user_id = @UserId;
            """;

        IEnumerable<Transaction> transactions = await connection.QueryAsync<Transaction>(
            sql,
            new { UserId = userId },
            transaction: transaction);

        return transactions.ToList();
    }
}
