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

    public async Task<int> CalculateNetPurchasedQuantityAsync(
        Guid userId, 
        string ticker,
        CancellationToken cancellationToken = default)
    {
        var transactions = await ApplySpecification(
            new CalculateNetPurchasedQuantitySpecification(userId, ticker))
            .ToListAsync(cancellationToken);

        return transactions.Sum(t =>
            t.Type == TransactionType.Expense ? t.Quantity : -t.Quantity);
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
