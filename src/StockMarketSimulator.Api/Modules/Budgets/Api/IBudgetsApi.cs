using Npgsql;

namespace StockMarketSimulator.Api.Modules.Budgets.Api;

public interface IBudgetsApi
{
    Task<BudgetApiResponse?> GetByIdAsync(
        NpgsqlConnection connection,
        Guid budgetId,
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default);

    Task<BudgetApiResponse?> GetByUserIdAsync(
        NpgsqlConnection connection,
        Guid userId,
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default);

    Task CreateAsync(
        NpgsqlConnection connection,
        BudgetApiResponse budgetApi,
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        NpgsqlConnection connection,
        BudgetApiResponse budgetApi,
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default);
}
