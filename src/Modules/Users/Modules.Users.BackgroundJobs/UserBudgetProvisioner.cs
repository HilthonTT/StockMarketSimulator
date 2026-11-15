using EntityFramework.Exceptions.Common;
using Modules.Budgeting.Api.Api;
using Modules.Users.Application.Abstractions.BackgroundJobs;

namespace Modules.Users.BackgroundJobs;

/// <summary>
/// Idempotent budget creation through the Budgeting module's public API.
/// </summary>
internal sealed class UserBudgetProvisioner(IBudgetingApi budgetingApi) : IUserBudgetProvisioner
{
    public async Task EnsureBudgetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (await budgetingApi.GetBudgetByUserIdAsync(userId, cancellationToken) is not null)
        {
            return;
        }

        try
        {
            await budgetingApi.AddBudgetAsync(userId, BudgetCurrencies.Default, cancellationToken);
        }
        catch (UniqueConstraintException)
        {
            // An earlier or concurrent attempt already created it (budgets are unique per user).
        }
    }
}
