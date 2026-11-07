using Application.Core.Extensions;
using FluentValidation;
using Modules.Budgeting.Application.Core.Errors;

namespace Modules.Budgeting.Application.Positions.GetByUserId;

internal sealed class GetPositionsByUserIdQueryValidator : AbstractValidator<GetPositionsByUserIdQuery>
{
    public GetPositionsByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithError(BudgetingValidationErrors.GetPositionsByUserId.UserIdIsRequired);
    }
}
