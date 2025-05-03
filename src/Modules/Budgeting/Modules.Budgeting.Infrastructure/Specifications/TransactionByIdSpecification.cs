using Infrastructure.Database.Specifications;
using Modules.Budgeting.Domain.Entities;

namespace Modules.Budgeting.Infrastructure.Specifications;

internal sealed class TransactionByIdSpecification : Specification<Transaction>
{
    public TransactionByIdSpecification(Guid id) 
        : base(transaction => transaction.Id == id)
    {
    }
}
