using Modules.Budgeting.Domain.Entities;
using SharedKernel;

namespace Modules.Budgeting.Domain.Repositories;

public interface IOrderRepository
{
    Task<Option<Order>> GetByIdAsync(Guid orderId, CancellationToken cancellationToken = default);

    Task<int> CountPendingAsync(Guid userId, CancellationToken cancellationToken = default);

    void Insert(Order order);
}
