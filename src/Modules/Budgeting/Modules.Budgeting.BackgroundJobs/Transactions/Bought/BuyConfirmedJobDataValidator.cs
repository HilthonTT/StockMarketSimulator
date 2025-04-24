using FluentValidation;

namespace Modules.Budgeting.BackgroundJobs.Transactions.Bought;

internal sealed class BuyConfirmedJobDataValidator : AbstractValidator<BuyConfirmedJobData>
{
    public BuyConfirmedJobDataValidator()
    {
        RuleFor(x => x.TransactionId).NotEmpty();

        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.UserEmail).NotEmpty().EmailAddress();

        RuleFor(x => x.Quantity).GreaterThan(0);

        RuleFor(x => x.TotalAmount).GreaterThan(0);
    }
}
