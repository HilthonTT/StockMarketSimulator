using Application.Core.Extensions;
using FluentValidation;
using Modules.Budgeting.Application.Core.Errors;

namespace Modules.Budgeting.Application.Transactions.GetByUserId;

internal sealed class GetTransactionsByUserIdQueryValidator : AbstractValidator<GetTransactionsByUserIdQuery>
{
    public GetTransactionsByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithError(BudgetingValidationErrors.GetTransactionsByUserId.UserIdIsRequired);

        RuleFor(x => x.Page)
            .NotEmpty().WithError(BudgetingValidationErrors.GetTransactionsByUserId.PageIsRequired)
            .GreaterThan(0).WithError(BudgetingValidationErrors.GetTransactionsByUserId.PageMustBePositive);

        RuleFor(x => x.PageSize)
            .NotEmpty().WithError(BudgetingValidationErrors.GetTransactionsByUserId.PageSizeIsRequired)
            .LessThanOrEqualTo(100).WithError(BudgetingValidationErrors.GetTransactionsByUserId.PageSizeMustBeAtMostOneHundred);
    }
}
