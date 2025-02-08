using FluentValidation;

namespace StockMarketSimulator.Api.Modules.Transactions.Application.Buy;

internal sealed class BuyTransactionCommandValidator : AbstractValidator<BuyTransactionCommand>
{
    public BuyTransactionCommandValidator()
    {
        RuleFor(x => x.Ticker)
            .NotEmpty().WithMessage("Ticker is required.")
            .MaximumLength(10).WithMessage("Ticker must be at most 10 characters");

        RuleFor(x => x.LimitPrice)
            .GreaterThan(0).WithMessage("Limit price must be greater than zero");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero");
    }
}
