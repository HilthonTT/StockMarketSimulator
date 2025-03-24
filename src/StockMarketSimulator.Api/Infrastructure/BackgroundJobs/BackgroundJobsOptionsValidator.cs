using FluentValidation;

namespace StockMarketSimulator.Api.Infrastructure.BackgroundJobs;

internal sealed class BackgroundJobsOptionsValidator : AbstractValidator<BackgroundJobsOptions>
{
    public BackgroundJobsOptionsValidator()
    {
        RuleFor(x => x.IntervalInSeconds).NotNull().GreaterThanOrEqualTo(1);
    }
}
