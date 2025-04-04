using FluentValidation;

namespace Modules.Budgeting.Application.Transactions.Sell;

internal sealed class SellTransactionCommandValidator : AbstractValidator<SellTransactionCommand>
{
    public SellTransactionCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.Ticker).MaximumLength(10);

        RuleFor(x => x.Quantity).GreaterThanOrEqualTo(1);
    }
}
