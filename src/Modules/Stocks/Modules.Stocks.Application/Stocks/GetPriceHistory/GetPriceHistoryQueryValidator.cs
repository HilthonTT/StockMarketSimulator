using Application.Core.Extensions;
using FluentValidation;
using Modules.Stocks.Application.Abstractions.Realtime;
using Modules.Stocks.Application.Core.Errors;

namespace Modules.Stocks.Application.Stocks.GetPriceHistory;

internal sealed class GetPriceHistoryQueryValidator : AbstractValidator<GetPriceHistoryQuery>
{
    public GetPriceHistoryQueryValidator()
    {
        RuleFor(x => x.Ticker)
            .NotEmpty().WithError(StocksValidationErrors.GetPriceHistory.TickerIsRequired)
            .MaximumLength(10).WithError(StocksValidationErrors.GetPriceHistory.TickerInvalidFormat);

        RuleFor(x => x.Range)
            .Must(range => PriceRange.FromName(range) is not null)
            .WithError(StocksValidationErrors.GetPriceHistory.RangeInvalid);
    }
}
