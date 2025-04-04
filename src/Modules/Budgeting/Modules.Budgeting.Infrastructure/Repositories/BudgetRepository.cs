using Microsoft.EntityFrameworkCore;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Repositories;
using Modules.Budgeting.Infrastructure.Database;

namespace Modules.Budgeting.Infrastructure.Repositories;

internal sealed class BudgetRepository(BudgetingDbContext context) : IBudgetRepository
{
    public Task<bool> AlreadyHasBudgetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return context.Budgets.AnyAsync(x => x.UserId == userId, cancellationToken);
    }

    public Task<Budget?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return context.Budgets.FirstOrDefaultAsync(x => x.UserId == userId, cancellationToken);
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
