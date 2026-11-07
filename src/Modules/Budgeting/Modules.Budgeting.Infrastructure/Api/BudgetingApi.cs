using Microsoft.EntityFrameworkCore;
using Modules.Budgeting.Api.Api;
using Modules.Budgeting.Api.Responses;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Enums;
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

    /// <summary>
    /// Returns the tickers the user currently holds (net bought quantity &gt; 0).
    /// </summary>
    public async Task<List<string>> GetPurchasedTickersAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Only the three columns needed are loaded; the type is a value-converted enumeration,
        // so the signed sum is computed in memory.
        var rows = await context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .Select(t => new { t.Ticker, t.Type, t.Quantity })
            .ToListAsync(cancellationToken);

        return [.. rows
            .GroupBy(r => r.Ticker)
            .Where(g => g.Sum(r => r.Type == TransactionType.Expense ? r.Quantity : -r.Quantity) > 0)
            .Select(g => g.Key)];
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
