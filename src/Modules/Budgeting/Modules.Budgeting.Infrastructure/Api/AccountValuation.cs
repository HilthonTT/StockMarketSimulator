using Microsoft.EntityFrameworkCore;
using Modules.Budgeting.Api.Responses;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Infrastructure.Database;
using Modules.Stocks.Api.Api;

namespace Modules.Budgeting.Infrastructure.Api;

/// <summary>Values every account (cash plus holdings at current prices) for the leaderboard.</summary>
internal static class AccountValuation
{
    public static async Task<List<AccountValueApiResponse>> GetAllAsync(
        BudgetingDbContext context,
        IStocksApi stocksApi,
        CancellationToken cancellationToken)
    {
        var budgets = await context.Budgets
            .AsNoTracking()
            .Select(b => new { b.UserId, Cash = b.Money.Amount })
            .ToListAsync(cancellationToken);

        var positions = await context.Positions
            .AsNoTracking()
            .Where(p => p.Quantity > 0)
            .Select(p => new { p.UserId, p.Ticker, p.Quantity })
            .ToListAsync(cancellationToken);

        IReadOnlyDictionary<string, decimal> prices = await stocksApi.GetPricesAsync(
            [.. positions.Select(p => p.Ticker).Distinct()],
            cancellationToken);

        var holdingsByUser = positions
            .GroupBy(p => p.UserId)
            .ToDictionary(
                g => g.Key,
                g => (Value: g.Sum(p => p.Quantity * prices.GetValueOrDefault(p.Ticker)), Count: g.Count()));

        return [.. budgets.Select(b =>
        {
            (decimal holdings, int count) = holdingsByUser.GetValueOrDefault(b.UserId);
            decimal accountValue = b.Cash + holdings;

            return new AccountValueApiResponse(
                b.UserId,
                b.Cash,
                Math.Round(holdings, 2),
                Math.Round(accountValue, 2),
                Math.Round((accountValue - Budget.InitialBudgetAmount) / Budget.InitialBudgetAmount, 6),
                count);
        })];
    }
}
