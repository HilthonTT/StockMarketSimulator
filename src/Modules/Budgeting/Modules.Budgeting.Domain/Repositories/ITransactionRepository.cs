using Modules.Budgeting.Domain.Entities;
using SharedKernel;

namespace Modules.Budgeting.Domain.Repositories;

public interface ITransactionRepository
{
    Task<Option<Transaction>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    void Insert(Transaction transaction);

    void Remove(Transaction transaction);
}
