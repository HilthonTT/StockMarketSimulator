using Application.Core.Extensions;
using FluentValidation;
using Modules.Budgeting.Application.Core.Errors;

namespace Modules.Budgeting.Application.Auditlogs.Delete;

internal sealed class DeleteAuditlogCommandValidator : AbstractValidator<DeleteAuditlogCommand>
{
    public DeleteAuditlogCommandValidator()
    {
        RuleFor(x => x.AuditlogId)
            .NotEmpty().WithError(BudgetingValidationErrors.DeleteAuditlog.AuditlogIdIsRequired);
    }
}
