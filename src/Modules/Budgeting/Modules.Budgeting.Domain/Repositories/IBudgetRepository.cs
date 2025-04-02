using Modules.Budgeting.Domain.Entities;

namespace Modules.Budgeting.Domain.Repositories;

public interface IBudgetRepository
{
    Task<Budget?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

    void Insert(Budget budget);

    void Remove(Budget budget);
}
