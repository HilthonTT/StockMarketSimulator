﻿using Application.Core.Extensions;
using FluentValidation;
using Modules.Stocks.Application.Core.Errors;

namespace Modules.Stocks.Application.Shorten.GetByTicker;

internal sealed class GetShortenUrlByTickerQueryValidator : AbstractValidator<GetShortenUrlByTickerQuery>
{
    public GetShortenUrlByTickerQueryValidator()
    {
        RuleFor(x => x.Ticker)
            .NotEmpty().WithError(StocksValidationErrors.GetShortenUrlByTicker.TickerIsRequired)
            .MaximumLength(10).WithError(StocksValidationErrors.GetShortenUrlByTicker.TickerInvalidFormat);
    }
}
