using Application.Core.Extensions;
using FluentValidation;
using Modules.Budgeting.Application.Core.Errors;

namespace Modules.Budgeting.Application.Transactions.Sell;

internal sealed class SellTransactionCommandValidator : AbstractValidator<SellTransactionCommand>
{
    public SellTransactionCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithError(BudgetingValidationErrors.SellTransaction.UserIdIsRequired);

        RuleFor(x => x.Ticker)
            .NotEmpty().WithError(BudgetingValidationErrors.SellTransaction.TickerIsRequired)
            .MaximumLength(10).WithError(BudgetingValidationErrors.SellTransaction.TickerInvalidFormat);

        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(1).WithError(BudgetingValidationErrors.SellTransaction.QuantityMustBeAtleastOne);
    }
}
