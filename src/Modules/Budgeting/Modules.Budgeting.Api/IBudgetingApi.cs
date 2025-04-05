namespace Modules.Budgeting.Api;

public interface IBudgetingApi
{
    Task<BudgetApiResponse?> GetBudgetByUserId(Guid userId, CancellationToken cancellationToken);

    Task AddBudgetAsync(Guid userId, CancellationToken cancellationToken = default);
}
