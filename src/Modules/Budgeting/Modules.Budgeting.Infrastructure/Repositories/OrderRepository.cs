using Microsoft.EntityFrameworkCore;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Enums;
using Modules.Budgeting.Domain.Repositories;
using Modules.Budgeting.Infrastructure.Database;
using SharedKernel;

namespace Modules.Budgeting.Infrastructure.Repositories;

internal sealed class OrderRepository(BudgetingDbContext context) : IOrderRepository
{
    public async Task<Option<Order>> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default)
    {
        Order? order = await context.Orders.FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        return Option<Order>.Some(order);
    }

    public Task<int> CountPendingAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        return context.Orders.CountAsync(o => o.UserId == userId && o.Status == OrderStatus.Pending, cancellationToken);
    }

    public void Insert(Order order)
    {
        context.Orders.Add(order);
    }
}
