using FluentValidation;

namespace StockMarketSimulator.Api.Modules.Stocks.Application.GetByTicker;

internal sealed class GetStockByTickerQueryValidator : AbstractValidator<GetStockByTickerQuery>
{
    public GetStockByTickerQueryValidator()
    {
        RuleFor(x => x.Ticker)
            .NotEmpty().WithMessage("Ticker is required")
            .MaximumLength(10).WithMessage("Ticker is at most 10 characters long");
    }
}
