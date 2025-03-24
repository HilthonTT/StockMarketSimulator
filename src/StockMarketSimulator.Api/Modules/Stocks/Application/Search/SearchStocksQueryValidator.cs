using FluentValidation;

namespace StockMarketSimulator.Api.Modules.Stocks.Application.Search;

internal sealed class SearchStocksQueryValidator : AbstractValidator<SearchStocksQuery>
{
    public SearchStocksQueryValidator()
    {
        RuleFor(x => x.SearchTerm)
            .NotEmpty().WithMessage("Search term is required");
    }
}
