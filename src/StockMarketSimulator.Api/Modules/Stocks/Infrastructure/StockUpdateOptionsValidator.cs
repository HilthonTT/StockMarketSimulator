using FluentValidation;

namespace StockMarketSimulator.Api.Modules.Stocks.Infrastructure;

internal sealed class StockUpdateOptionsValidator : AbstractValidator<StockUpdateOptions>
{
    public StockUpdateOptionsValidator()
    {
        RuleFor(x => x.UpdateIntervalInSeconds).NotNull().GreaterThanOrEqualTo(1);

        RuleFor(x => x.MaxPercentageChange).NotNull().GreaterThan(0);
    }
}
