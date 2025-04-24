using FluentValidation;

namespace Modules.Budgeting.BackgroundJobs.Transactions.Sold;

public sealed class SellConfirmedJobDataValidator : AbstractValidator<SellConfirmedJobData>
{
    public SellConfirmedJobDataValidator()
    {
        RuleFor(x => x.Ticker)
            .NotEmpty().WithMessage("Ticker must not be empty.");

        RuleFor(x => x.TransactionId)
            .NotEmpty().WithMessage("TransactionId is required.");

        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("UserId is required.");

        RuleFor(x => x.UserEmail)
            .NotEmpty().EmailAddress().WithMessage("A valid email is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0).WithMessage("Quantity must be greater than zero.");

        RuleFor(x => x.TotalAmount)
            .GreaterThan(0).WithMessage("Total amount must be greater than zero.");
    }
}
