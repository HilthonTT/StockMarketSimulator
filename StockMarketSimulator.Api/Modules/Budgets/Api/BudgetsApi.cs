using Npgsql;
using StockMarketSimulator.Api.Modules.Budgets.Domain;

namespace StockMarketSimulator.Api.Modules.Budgets.Api;

internal sealed class BudgetsApi : IBudgetsApi
{
    private readonly IBudgetRepository _budgetRepository;

    public BudgetsApi(IBudgetRepository budgetRepository)
    {
        _budgetRepository = budgetRepository;
    }

    public Task CreateAsync(
        NpgsqlConnection connection, 
        BudgetApiResponse budgetApi, 
        NpgsqlTransaction? transaction = null, 
        CancellationToken cancellationToken = default)
    {
        var budget = new Budget
        {
            Id = budgetApi.Id,
            UserId = budgetApi.UserId,
            BuyingPower = budgetApi.BuyingPower,
        };

        return _budgetRepository.CreateAsync(connection, budget, transaction, cancellationToken);
    }

    public async Task<BudgetApiResponse?> GetByIdAsync(
        NpgsqlConnection connection, 
        Guid budgetId, 
        NpgsqlTransaction? transaction = null, 
        CancellationToken cancellationToken = default)
    {
        Budget? budget = await _budgetRepository.GetByIdAsync(connection, budgetId, transaction, cancellationToken);
        if (budget is null)
        {
            return null;
        }

        return new BudgetApiResponse(budget.Id, budget.UserId, budget.BuyingPower);
    }

    public async Task<BudgetApiResponse?> GetByUserIdAsync(
        NpgsqlConnection connection, 
        Guid userId, 
        NpgsqlTransaction? transaction = null, 
        CancellationToken cancellationToken = default)
    {
        Budget? budget = await _budgetRepository.GetByUserIdAsync(connection, userId, transaction, cancellationToken);
        if (budget is null)
        {
            return null;
        }

        return new BudgetApiResponse(budget.Id, budget.UserId, budget.BuyingPower);
    }

    public Task UpdateAsync(
        NpgsqlConnection connection, 
        BudgetApiResponse budgetApi, 
        NpgsqlTransaction? transaction = null, 
        CancellationToken cancellationToken = default)
    {
        var budget = new Budget
        {
            Id = budgetApi.Id,
            UserId = budgetApi.UserId,
            BuyingPower = budgetApi.BuyingPower,
        };

        return _budgetRepository.UpdateAsync(connection, budget, transaction, cancellationToken);
    }
}
