using Application.Abstractions.Messaging;
using Microsoft.EntityFrameworkCore;
using Modules.Budgeting.Application.Abstractions.Data;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Enums;
using Modules.Stocks.Api.Api;
using SharedKernel;

namespace Modules.Budgeting.Application.Orders.GetTriggered;

internal sealed class GetTriggeredOrderIdsQueryHandler(
    IDbContext dbContext,
    IStocksApi stocksApi) : IQueryHandler<GetTriggeredOrderIdsQuery, List<Guid>>
{
    public async Task<Result<List<Guid>>> Handle(GetTriggeredOrderIdsQuery request, CancellationToken cancellationToken)
    {
        List<Order> pending = await dbContext.Orders
            .AsNoTracking()
            .Where(o => o.Status == OrderStatus.Pending)
            .OrderBy(o => o.CreatedOnUtc)
            .Take(request.BatchSize)
            .ToListAsync(cancellationToken);

        if (pending.Count == 0)
        {
            return new List<Guid>();
        }

        IReadOnlyDictionary<string, decimal> prices = await stocksApi.GetPricesAsync(
            [.. pending.Select(o => o.Ticker).Distinct()],
            cancellationToken);

        return pending
            .Where(o => prices.TryGetValue(o.Ticker, out decimal price) && o.IsTriggeredBy(price))
            .Select(o => o.Id)
            .ToList();
    }
}
