using Modules.Budgeting.Api.Responses;

namespace Modules.Budgeting.Api.Api;

public interface IBudgetingApi
{
    Task<BudgetApiResponse?> GetBudgetByUserIdAsync(Guid userId, CancellationToken cancellationToken);

    Task<int> AddBudgetAsync(Guid userId, string currencyCode, CancellationToken cancellationToken = default);

    Task<List<TransactionApiResponse>> GetTransactionsByUserIdAsync(
        Guid userId, 
        CancellationToken cancellationToken = default);

    Task<List<string>> GetPurchasedTickersAsync(Guid userId, CancellationToken cancellationToken = default);
}
