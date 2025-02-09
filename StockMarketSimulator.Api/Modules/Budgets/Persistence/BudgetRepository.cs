using Dapper;
using Npgsql;
using StockMarketSimulator.Api.Modules.Budgets.Domain;

namespace StockMarketSimulator.Api.Modules.Budgets.Persistence;

internal sealed class BudgetRepository : IBudgetRepository
{
    public Task CreateAsync(
        NpgsqlConnection connection, 
        Budget budget, 
        NpgsqlTransaction? transaction = null, 
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            INSERT INTO public.budgets(id, user_id, buying_power)
            VALUES (@Id, @UserId, @BuyingPower);
            """;

        return connection.ExecuteAsync(
            sql,
            new
            {
                Id = budget.Id,
                UserId = budget.UserId,
                BuyingPower = budget.BuyingPower,
            },
            transaction: transaction);
    }

    public Task<Budget?> GetByIdAsync(
        NpgsqlConnection connection, 
        Guid budgetId, 
        NpgsqlTransaction? transaction = null, 
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT
                id AS Id,
                user_id AS UserId,
                buying_power AS BuyingPower
            FROM public.budgets
            WHERE id = @BudgetId;
            """;

        return connection.QueryFirstOrDefaultAsync<Budget>(
            sql,
            new
            {
                Id = budgetId,
            },
            transaction: transaction);
    }

    public Task<Budget?> GetByUserIdAsync(
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
                buying_power AS BuyingPower
            FROM public.budgets
            WHERE user_id = @UserId;
            """;

        return connection.QueryFirstOrDefaultAsync<Budget>(
            sql,
            new
            {
                UserId = userId,
            },
            transaction: transaction);
    }

    public Task UpdateAsync(
        NpgsqlConnection connection, 
        Budget budget, 
        NpgsqlTransaction? transaction = null, 
        CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            UPDATE public.budgets
            SET buying_power = @BuyingPower
            WHERE id = @Id;
            """;

        return connection.ExecuteAsync(
            sql,
            new
            {
                Id = budget.Id,
                BuyingPower = budget.BuyingPower
            },
            transaction: transaction);
    }
}
