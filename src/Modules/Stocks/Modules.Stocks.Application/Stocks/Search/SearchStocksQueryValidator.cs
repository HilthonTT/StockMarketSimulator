using Application.Core.Extensions;
using FluentValidation;
using Modules.Stocks.Application.Core.Errors;

namespace Modules.Stocks.Application.Stocks.Search;

internal sealed class SearchStocksQueryValidator : AbstractValidator<SearchStocksQuery>
{
    public SearchStocksQueryValidator()
    {
        RuleFor(x => x.Page)
            .NotEmpty().WithError(ValidationErrors.SearchStocks.PageIsRequired)
            .GreaterThan(0).WithError(ValidationErrors.SearchStocks.PageMustBeGreaterThanZero);

        RuleFor(x => x.PageSize)
            .NotEmpty().WithError(ValidationErrors.SearchStocks.PageSizeIsRequired)
            .GreaterThan(0).WithError(ValidationErrors.SearchStocks.PageSizeMustBeInRange)
            .LessThanOrEqualTo(100).WithError(ValidationErrors.SearchStocks.PageSizeMustBeInRange);
    }
}
