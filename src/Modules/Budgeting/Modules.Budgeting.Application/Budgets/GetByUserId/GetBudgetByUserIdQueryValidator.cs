using Application.Core.Extensions;
using FluentValidation;
using Modules.Budgeting.Application.Core.Errors;

namespace Modules.Budgeting.Application.Budgets.GetByUserId;

internal sealed class GetBudgetByUserIdQueryValidator : AbstractValidator<GetBudgetByUserIdQuery>
{
    public GetBudgetByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithError(ValidationErrors.GetBudgetByUserId.UserIdIsRequired);
    }
}
