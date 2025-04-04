using FluentValidation;

namespace Modules.Budgeting.Application.Transactions.Buy;

internal sealed class BuyTransactionCommandValidator : AbstractValidator<BuyTransactionCommand>
{
    public BuyTransactionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.Ticker).NotEmpty().MaximumLength(10);

        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(1);
    }
}
