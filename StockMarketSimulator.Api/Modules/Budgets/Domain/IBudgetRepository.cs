using Npgsql;

namespace StockMarketSimulator.Api.Modules.Budgets.Domain;

internal interface IBudgetRepository
{
    Task<Budget?> GetByIdAsync(
        NpgsqlConnection connection, 
        Guid budgetId, 
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default);

    Task<Budget?> GetByUserIdAsync(
        NpgsqlConnection connection,
        Guid userId,
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default);

    Task CreateAsync(
        NpgsqlConnection connection,
        Budget budget,
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        NpgsqlConnection connection,
        Budget budget,
        NpgsqlTransaction? transaction = null,
        CancellationToken cancellationToken = default);
}
