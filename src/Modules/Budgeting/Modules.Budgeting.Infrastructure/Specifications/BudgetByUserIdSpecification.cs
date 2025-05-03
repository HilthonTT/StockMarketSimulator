using Infrastructure.Database.Specifications;
using Modules.Budgeting.Domain.Entities;

namespace Modules.Budgeting.Infrastructure.Specifications;

internal sealed class BudgetByUserIdSpecification : Specification<Budget>
{
    public BudgetByUserIdSpecification(Guid userId) 
        : base(budget => budget.UserId == userId)
    {
    }
}
