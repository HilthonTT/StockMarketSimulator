using FluentValidation;

namespace StockMarketSimulator.Api.Modules.Users.Application.GetById;

internal sealed class GetUserByIdQueryValidator : AbstractValidator<GetUserByIdQuery>
{
    public GetUserByIdQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("The user identifier is required");
    }
}
