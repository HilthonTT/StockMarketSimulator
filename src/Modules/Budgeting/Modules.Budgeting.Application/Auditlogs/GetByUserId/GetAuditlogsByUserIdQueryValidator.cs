using Application.Core.Extensions;
using FluentValidation;
using Modules.Budgeting.Application.Core.Errors;

namespace Modules.Budgeting.Application.Auditlogs.GetByUserId;

internal sealed class GetAuditlogsByUserIdQueryValidator : AbstractValidator<GetAuditlogsByUserIdQuery>
{
    public GetAuditlogsByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithError(BudgetingValidationErrors.GetAuditlogsByUserId.UserIdIsRequired);

        RuleFor(x => x.Page)
            .NotEmpty().WithError(BudgetingValidationErrors.GetAuditlogsByUserId.PageIsRequired)
            .GreaterThan(0).WithError(BudgetingValidationErrors.GetAuditlogsByUserId.PageMustBePositive);

        RuleFor(x => x.PageSize)
            .NotEmpty().WithError(BudgetingValidationErrors.GetAuditlogsByUserId.PageSizeIsRequired)
            .LessThanOrEqualTo(100).WithError(BudgetingValidationErrors.GetAuditlogsByUserId.PageSizeMustBeAtMostOneHundred);
    }
}
