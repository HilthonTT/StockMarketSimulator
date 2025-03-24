using FluentValidation;

namespace StockMarketSimulator.Api.Modules.Budgets.Application.GetByUserId;

internal sealed class GetBudgetByUserIdQueryValidator : AbstractValidator<GetBudgetByUserIdQuery>
{
    public GetBudgetByUserIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("The user identifier is required");
    }
}
