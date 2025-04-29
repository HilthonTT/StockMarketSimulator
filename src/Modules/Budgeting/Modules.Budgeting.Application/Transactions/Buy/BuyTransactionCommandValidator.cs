using Application.Core.Extensions;
using FluentValidation;
using Modules.Budgeting.Application.Core.Errors;

namespace Modules.Budgeting.Application.Transactions.Buy;

internal sealed class BuyTransactionCommandValidator : AbstractValidator<BuyTransactionCommand>
{
    public BuyTransactionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithError(BudgetingValidationErrors.BuyTransaction.UserIdIsRequired);

        RuleFor(x => x.Ticker)
            .NotEmpty().WithError(BudgetingValidationErrors.BuyTransaction.TickerIsRequired)
            .MaximumLength(10).WithError(BudgetingValidationErrors.BuyTransaction.TickerInvalidFormat);

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(1).WithError(BudgetingValidationErrors.BuyTransaction.QuantityMustBeAtleastOne);
    }
}
