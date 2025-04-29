using Application.Core.Extensions;
using FluentValidation;
using Modules.Stocks.Application.Core.Errors;

namespace Modules.Stocks.Application.Stocks.Search;

internal sealed class SearchStocksQueryValidator : AbstractValidator<SearchStocksQuery>
{
    public SearchStocksQueryValidator()
    {
        RuleFor(x => x.Page)
            .NotEmpty().WithError(StocksValidationErrors.SearchStocks.PageIsRequired)
            .GreaterThan(0).WithError(StocksValidationErrors.SearchStocks.PageMustBeGreaterThanZero);

        RuleFor(x => x.PageSize)
            .NotEmpty().WithError(StocksValidationErrors.SearchStocks.PageSizeIsRequired)
            .GreaterThan(0).WithError(StocksValidationErrors.SearchStocks.PageSizeMustBeInRange)
            .LessThanOrEqualTo(100).WithError(StocksValidationErrors.SearchStocks.PageSizeMustBeInRange);
    }
}
