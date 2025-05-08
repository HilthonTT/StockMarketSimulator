using Application.Core.Extensions;
using FluentValidation;
using Modules.Stocks.Application.Core.Errors;

namespace Modules.Stocks.Application.Stocks.GetPurchasedStockTickers;

internal sealed class GetPurchasedStockTickersQueryValidator : AbstractValidator<GetPurchasedStockTickersQuery>
{
    public GetPurchasedStockTickersQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithError(StocksValidationErrors.GetPurchasedStockTickers.UserIdIsRequired);
    }
}
