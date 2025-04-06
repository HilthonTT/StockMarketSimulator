using Modules.Budgeting.Domain.Entities;
using SharedKernel;

namespace Modules.Budgeting.Domain.Repositories;

public interface IBudgetRepository
{
    Task<Option<Budget>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<bool> AlreadyHasBudgetAsync(Guid userId, CancellationToken cancellationToken = default);

    void Insert(Budget budget);

    void Remove(Budget budget);
}
