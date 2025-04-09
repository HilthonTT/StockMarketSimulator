using FluentValidation;

namespace Modules.Budgeting.Application.Budgets.GetByUserId;

internal sealed class GetBudgetByUserIdQueryValidator : AbstractValidator<GetBudgetByUserIdQuery>
{
    public GetBudgetByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
