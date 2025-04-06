using Microsoft.EntityFrameworkCore;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Repositories;
using Modules.Budgeting.Infrastructure.Database;
using SharedKernel;

namespace Modules.Budgeting.Infrastructure.Repositories;

internal sealed class BudgetRepository(BudgetingDbContext context) : IBudgetRepository
{
    public Task<bool> AlreadyHasBudgetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return context.Budgets.AnyAsync(x => x.UserId == userId, cancellationToken);
    }

    public async Task<Option<Budget>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        Budget? budget = await context.Budgets.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);

        return Option<Budget>.Some(budget);
    }

    public void Insert(Budget budget)
    {
        context.Budgets.Add(budget);
    }

    public void Remove(Budget budget)
    {
        context.Budgets.Remove(budget);
    }
}
