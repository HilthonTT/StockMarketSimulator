using FluentValidation;

namespace StockMarketSimulator.Api.Modules.Transactions.Application.Create;

internal sealed class CreateTransactionCommandValidator : AbstractValidator<CreateTransactionCommand>
{
    public CreateTransactionCommandValidator()
    {
        RuleFor(x => x.Ticker)
            .NotEmpty().WithMessage("Ticker is required.")
            .MaximumLength(10).WithMessage("Ticker must be at most 10 characters");

        RuleFor(x => x.LimitPrice)
            .GreaterThan(0).WithMessage("Limit price must be greater than zero");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero");

        RuleFor(x => x.Type)
            .IsInEnum().WithMessage("Invalid transaction type");
    }
}
