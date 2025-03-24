using FluentValidation;

namespace StockMarketSimulator.Api.Modules.Transactions.Application.Sell;

internal sealed class SellTransactionCommandValidator : AbstractValidator<SellTransactionCommand>
{
    public SellTransactionCommandValidator()
    {
        RuleFor(x => x.Ticker)
           .NotEmpty().WithMessage("Ticker is required.")
           .MaximumLength(10).WithMessage("Ticker must be at most 10 characters");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero");
    }
}
