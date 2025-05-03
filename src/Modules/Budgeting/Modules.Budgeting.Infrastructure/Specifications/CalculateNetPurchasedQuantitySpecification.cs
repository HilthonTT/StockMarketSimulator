using Infrastructure.Database.Specifications;
using Modules.Budgeting.Domain.Entities;

namespace Modules.Budgeting.Infrastructure.Specifications;

internal sealed class CalculateNetPurchasedQuantitySpecification :
    Specification<Transaction>
{
    public CalculateNetPurchasedQuantitySpecification(Guid userId, string ticker) 
        : base(transaction => transaction.UserId == userId && transaction.Ticker == ticker)
    {
    }
}
