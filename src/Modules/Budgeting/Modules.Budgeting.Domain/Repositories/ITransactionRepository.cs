using Modules.Budgeting.Domain.Entities;
using SharedKernel;

namespace Modules.Budgeting.Domain.Repositories;

public interface ITransactionRepository
{
    Task<int> CalculateNetPurchasedQuantityAsync(Guid userId, string ticker, CancellationToken cancellationToken = default);

    Task<Option<Transaction>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Insert(Transaction transaction);

    void Remove(Transaction transaction);
}
