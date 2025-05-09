using System.Data;
using Dapper;
using Modules.Budgeting.Contracts.Transactions;

namespace Modules.Budgeting.Application.Transactions;

internal static class TransactionQueries
{
    public static async Task<TransactionCountResponse> GetTransactionCountAsync(IDbConnection connection, Guid userId)
    {
        const string sql =
            """
            SELECT COUNT(*) 
            FROM budgeting.transactions 
            WHERE user_id = @UserId;
            """;

        int count = await connection.ExecuteScalarAsync<int>(sql, new { UserId = userId });

        return new TransactionCountResponse(count);
    }
}
