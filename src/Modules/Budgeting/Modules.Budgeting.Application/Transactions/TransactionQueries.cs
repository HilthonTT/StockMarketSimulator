using System.Data;
using Dapper;
using Modules.Budgeting.Contracts.Transactions;

namespace Modules.Budgeting.Application.Transactions;

internal static class TransactionQueries
{
    public static async Task<TransactionCountResponse> GetTransactionCountAsync(
        IDbConnection connection, 
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT COUNT(*) 
            FROM budgeting.transactions 
            WHERE user_id = @UserId;
            """;

        int count = await connection.ExecuteScalarAsync<int>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));

        return new TransactionCountResponse(count);
    }
}
