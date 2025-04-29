using Application.Core.Extensions;
using FluentValidation;
using Modules.Stocks.Application.Core.Errors;

namespace Modules.Stocks.Application.Shorten.Get;

internal sealed class GetShortenUrlQueryValidator : AbstractValidator<GetShortenUrlQuery>
{
    public GetShortenUrlQueryValidator()
    {
        RuleFor(x => x.ShortCode)
            .NotEmpty().WithError(StocksValidationErrors.GetShortenUrlByShortCode.ShortCodeIsRequired);
    }
}
