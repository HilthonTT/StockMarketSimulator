using Application.Core.Extensions;
using FluentValidation;
using Modules.Budgeting.Application.Core.Errors;

namespace Modules.Budgeting.Application.Orders.Place;

internal sealed class PlaceOrderCommandValidator : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithError(BudgetingValidationErrors.PlaceOrder.UserIdIsRequired);

        RuleFor(x => x.Ticker)
            .NotEmpty().WithError(BudgetingValidationErrors.PlaceOrder.TickerIsRequired)
            .MaximumLength(10).WithError(BudgetingValidationErrors.PlaceOrder.TickerInvalidFormat);

        RuleFor(x => x.Side)
            .Must(side => OrderMapping.TryParseSide(side, out _))
            .WithError(BudgetingValidationErrors.PlaceOrder.SideInvalid);

        RuleFor(x => x.Type)
            .Must(type => OrderMapping.TryParseType(type, out _))
            .WithError(BudgetingValidationErrors.PlaceOrder.TypeInvalid);

        RuleFor(x => x.Quantity)
            .InclusiveBetween(1, 1_000_000).WithError(BudgetingValidationErrors.PlaceOrder.QuantityOutOfRange);

        RuleFor(x => x.TriggerPrice)
            .GreaterThan(0).WithError(BudgetingValidationErrors.PlaceOrder.TriggerPriceMustBePositive);
    }
}
