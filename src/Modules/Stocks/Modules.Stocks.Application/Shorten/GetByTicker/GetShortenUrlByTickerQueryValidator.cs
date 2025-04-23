using FluentValidation;

namespace Modules.Stocks.Application.Shorten.GetByTicker;

internal sealed class GetShortenUrlByTickerQueryValidator : AbstractValidator<GetShortenUrlByTickerQuery>
{
    public GetShortenUrlByTickerQueryValidator()
    {
        RuleFor(x => x.Ticker).NotEmpty().MaximumLength(10);
    }
}
