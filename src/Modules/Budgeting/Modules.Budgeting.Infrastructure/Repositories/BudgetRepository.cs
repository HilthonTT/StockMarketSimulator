using Infrastructure.Database.Specifications;
using Microsoft.EntityFrameworkCore;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Repositories;
using Modules.Budgeting.Infrastructure.Database;
using Modules.Budgeting.Infrastructure.Specifications;
using SharedKernel;

namespace Modules.Budgeting.Infrastructure.Repositories;

internal sealed class BudgetRepository(BudgetingDbContext context) : IBudgetRepository
{
    public async Task<Option<Budget>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        Budget? budget = await ApplySpecification(new BudgetByUserIdSpecification(userId))
            .FirstOrDefaultAsync(cancellationToken);

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

    private IQueryable<Budget> ApplySpecification(Specification<Budget> specification)
    {
        return SpecificationEvaluator.GetQuery(context.Budgets, specification);
    }
}
