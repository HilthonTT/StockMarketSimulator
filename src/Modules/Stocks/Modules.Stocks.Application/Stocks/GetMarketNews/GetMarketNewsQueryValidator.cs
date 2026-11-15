using Application.Core.Extensions;
using FluentValidation;
using Modules.Stocks.Application.Core.Errors;

namespace Modules.Stocks.Application.Stocks.GetMarketNews;

internal sealed class GetMarketNewsQueryValidator : AbstractValidator<GetMarketNewsQuery>
{
    public GetMarketNewsQueryValidator()
    {
        RuleFor(x => x.Limit)
            .InclusiveBetween(1, 100).WithError(StocksValidationErrors.GetMarketNews.LimitOutOfRange);
    }
}
