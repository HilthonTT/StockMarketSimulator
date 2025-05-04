using Microsoft.EntityFrameworkCore;
using Modules.Budgeting.Api;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Infrastructure.Database;

namespace Modules.Budgeting.Infrastructure.Api;

internal sealed class BudgetingApi(BudgetingDbContext context) : IBudgetingApi
{
    public Task<int> AddBudgetAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        Budget budget = Budget.Create(userId);

        context.Budgets.Add(budget);

        return context.SaveChangesAsync(cancellationToken);
    }

    public Task<BudgetApiResponse?> GetBudgetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return context.Budgets
            .AsNoTracking()
            .Select(b => new BudgetApiResponse(b.Id, b.UserId, b.BuyingPower))
            .FirstOrDefaultAsync(b => b.UserId == userId, cancellationToken);
    }

    public Task<List<TransactionApiResponse>> GetTransactionsByUserIdAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        return context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .Select(t => new TransactionApiResponse(t.Id, t.Ticker, (int)t.Type, t.LimitPrice))
            .ToListAsync(cancellationToken);
    }
}
