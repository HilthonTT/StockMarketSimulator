using System.Data;
using Dapper;
using Modules.Budgeting.Contracts.Budgets;
using SharedKernel;

namespace Modules.Budgeting.Application.Budgets;

internal static class BudgetQueries
{
    public async static Task<Option<BudgetResponse>> GetByUserIdAsync(
        IDbConnection connection,
        Guid userId)
    {
        const string sql =
           """
            SELECT
                id AS Id,
                user_id AS UserId,
                buying_power AS BuyingPower
            FROM budgeting.budgets
            WHERE user_id = @UserId
            LIMIT 1;
            """;

        BudgetResponse? budget = await connection.QueryFirstOrDefaultAsync<BudgetResponse>(sql, new
        {
            UserId = userId,
        });

        return Option<BudgetResponse>.Some(budget);
    }
}
