using Microsoft.EntityFrameworkCore;
using Modules.Budgeting.Api;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Infrastructure.Database;

namespace Modules.Budgeting.Infrastructure.Api;

internal sealed class BudgetingApi(BudgetingDbContext context) : IBudgetingApi
{
    public Task AddBudgetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        Budget budget = Budget.Create(userId);

        context.Budgets.Add(budget);

        return context.SaveChangesAsync(cancellationToken);
    }

    public Task<BudgetApiResponse?> GetBudgetByUserId(Guid userId, CancellationToken cancellationToken)
    {
        return context.Budgets
            .Select(b => new BudgetApiResponse(b.Id, b.UserId, b.BuyingPower))
            .FirstOrDefaultAsync(b => b.UserId == userId, cancellationToken);
    }
}
