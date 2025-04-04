using Microsoft.EntityFrameworkCore;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Repositories;
using Modules.Budgeting.Infrastructure.Database;

namespace Modules.Budgeting.Infrastructure.Repositories;

internal sealed class TransactionRepository(BudgetingDbContext context) : ITransactionRepository
{
    public Task<Transaction?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return context.Transactions.FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public void Insert(Transaction transaction)
    {
        context.Transactions.Add(transaction);
    }

    public void Remove(Transaction transaction)
    {
        context.Transactions.Remove(transaction);
    }
}
