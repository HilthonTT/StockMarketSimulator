using Infrastructure.Database.Specifications;
using Microsoft.EntityFrameworkCore;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Enums;
using Modules.Budgeting.Domain.Repositories;
using Modules.Budgeting.Infrastructure.Database;
using Modules.Budgeting.Infrastructure.Specifications;
using SharedKernel;

namespace Modules.Budgeting.Infrastructure.Repositories;

internal sealed class TransactionRepository(BudgetingDbContext context) : ITransactionRepository
{
    public async Task<Option<Transaction>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        Transaction? transaction = await ApplySpecification(new TransactionByIdSpecification(id))
            .FirstOrDefaultAsync(cancellationToken);

        return Option<Transaction>.Some(transaction);
    }

    public Task<int> CalculateNetPurchasedQuantityAsync(
        Guid userId, 
        string ticker,
        CancellationToken cancellationToken = default)
    {
        return ApplySpecification(new CalculateNetPurchasedQuantitySpecification(userId, ticker))
            .SumAsync(t => t.Type == TransactionType.Buy ? t.Quantity : -t.Quantity, cancellationToken);
    }

    public void Insert(Transaction transaction)
    {
        context.Transactions.Add(transaction);
    }

    public void Remove(Transaction transaction)
    {
        context.Transactions.Remove(transaction);
    }

    private IQueryable<Transaction> ApplySpecification(Specification<Transaction> specification)
    {
        return SpecificationEvaluator.GetQuery(context.Transactions, specification);
    }
}
