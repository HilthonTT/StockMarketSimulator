using Microsoft.EntityFrameworkCore;
using Modules.Budgeting.Api.Api;
using Modules.Budgeting.Api.Responses;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.ValueObjects;
using Modules.Budgeting.Infrastructure.Database;

namespace Modules.Budgeting.Infrastructure.Api;

internal sealed class BudgetingApi(BudgetingDbContext context) : IBudgetingApi
{
    public Task<int> AddBudgetAsync(Guid userId, string currencyCode, CancellationToken cancellationToken = default)
    {
        Budget budget = Budget.Create(userId, Currency.FromCode(currencyCode));

        context.Budgets.Add(budget);

        return context.SaveChangesAsync(cancellationToken);
    }

    public Task<BudgetApiResponse?> GetBudgetByUserIdAsync(Guid userId, CancellationToken cancellationToken)
    {
        return context.Budgets
            .AsNoTracking()
            .Select(b => new BudgetApiResponse(b.Id, b.UserId, b.Money.Amount, b.Money.Currency.Code))
            .FirstOrDefaultAsync(b => b.UserId == userId, cancellationToken);
    }

    public Task<List<TransactionApiResponse>> GetTransactionsByUserIdAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        return context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .Select(t => new TransactionApiResponse(t.Id, t.Ticker, t.Type.Id, t.Money.Amount, t.Money.Currency.Code))
            .ToListAsync(cancellationToken);
    }
}
