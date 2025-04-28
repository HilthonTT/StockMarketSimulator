using Application.Core.Extensions;
using FluentValidation;
using Modules.Budgeting.Application.Core.Errors;

namespace Modules.Budgeting.Application.Transactions.Sell;

internal sealed class SellTransactionCommandValidator : AbstractValidator<SellTransactionCommand>
{
    public SellTransactionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithError(ValidationErrors.SellTransaction.UserIdIsRequired);

        RuleFor(x => x.Ticker)
            .NotEmpty().WithError(ValidationErrors.SellTransaction.TickerIsRequired)
            .MaximumLength(10).WithError(ValidationErrors.SellTransaction.TickerInvalidFormat);

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(1).WithError(ValidationErrors.SellTransaction.QuantityMustBeAtleastOne);
    }
}
