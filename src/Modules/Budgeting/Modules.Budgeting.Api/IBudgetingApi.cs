namespace Modules.Budgeting.Api;

public interface IBudgetingApi
{
    Task<BudgetApiResponse?> GetBudgetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<int> AddBudgetAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<List<TransactionApiResponse>> GetTransactionsByUserIdAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);
}
