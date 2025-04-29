using Application.Core.Extensions;
using FluentValidation;
using Modules.Stocks.Application.Core.Errors;

namespace Modules.Stocks.Application.Stocks.GetByTicker;

internal sealed class GetStockByTickerQueryValidator : AbstractValidator<GetStockByTickerQuery>
{
    public GetStockByTickerQueryValidator()
    {
        RuleFor(x => x.Ticker)
             .NotEmpty().WithError(StocksValidationErrors.GetStockByTicker.TickerIsRequired)
             .MaximumLength(10).WithError(StocksValidationErrors.GetStockByTicker.TickerInvalidFormat);
    }
}
