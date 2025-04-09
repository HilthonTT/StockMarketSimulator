using FluentValidation;

namespace Modules.Stocks.Application.Stocks.GetByTicker;

internal sealed class GetStockByTickerQueryValidator : AbstractValidator<GetStockByTickerQuery>
{
    public GetStockByTickerQueryValidator()
    {
        RuleFor(x => x.Ticker).NotEmpty().MaximumLength(10);
    }
}
