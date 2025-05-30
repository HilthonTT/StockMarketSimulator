﻿using System.Data;
using Dapper;
using Modules.Budgeting.Contracts.Budgets;
using SharedKernel;

namespace Modules.Budgeting.Application.Budgets;

internal static class BudgetQueries
{
    public async static Task<Option<BudgetResponse>> GetByUserIdAsync(
        IDbConnection connection,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        const string sql =
           """
            SELECT
                id AS Id,
                user_id AS UserId,
                money_amount AS Amount,
                money_currency AS CurrencyCode
            FROM budgeting.budgets
            WHERE user_id = @UserId
            LIMIT 1;
            """;

        BudgetResponse? budget = await connection.QueryFirstOrDefaultAsync<BudgetResponse>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));

        return Option<BudgetResponse>.Some(budget);
    }
}
