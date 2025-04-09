using FluentValidation;

namespace Modules.Stocks.Application.Stocks.Search;

internal sealed class SearchStocksQueryValidator : AbstractValidator<SearchStocksQuery>
{
    public SearchStocksQueryValidator()
    {
        RuleFor(x => x.Page).NotEmpty().GreaterThan(0);

        RuleFor(x => x.PageSize).NotEmpty().GreaterThan(0);
    }
}
