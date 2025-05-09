using Application.Core.Extensions;
using FluentValidation;
using Modules.Budgeting.Application.Core.Errors;

namespace Modules.Budgeting.Application.Transactions.GetTransactionCount;

internal sealed class GetTransactionCountQueryValidator : AbstractValidator<GetTransactionCountQuery>
{
    public GetTransactionCountQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithError(BudgetingValidationErrors.GetTransactionCount.UserIdIsRequired);
    }
}
