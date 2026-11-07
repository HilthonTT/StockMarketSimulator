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

    /// <summary>
    /// Returns the tickers the user currently holds (position quantity &gt; 0).
    /// </summary>
    public Task<List<string>> GetPurchasedTickersAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return context.Positions
            .AsNoTracking()
            .Where(p => p.UserId == userId && p.Quantity > 0)
            .OrderBy(p => p.Ticker)
            .Select(p => p.Ticker)
            .ToListAsync(cancellationToken);
    }

    public async Task<List<PositionApiResponse>> GetOpenPositionsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var positions = await context.Positions
            .AsNoTracking()
            .Where(p => p.UserId == userId && p.Quantity > 0)
            .OrderBy(p => p.Ticker)
            .Select(p => new { p.Ticker, p.Quantity, p.TotalCost })
            .ToListAsync(cancellationToken);

        return [.. positions.Select(p => new PositionApiResponse(
            p.Ticker,
            p.Quantity,
            Math.Round(p.TotalCost / p.Quantity, Position.CostDecimals),
            p.TotalCost))];
    }

    public Task<List<TransactionApiResponse>> GetTransactionsByUserIdAsync(
        Guid userId, 
        CancellationToken cancellationToken = default)
    {
        return context.Transactions
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .Select(t => new TransactionApiResponse(t.Id, t.Ticker, t.Type.Id, t.Money.Amount, t.Money.Currency.Code, t.Quantity))
            .ToListAsync(cancellationToken);
    }
}
