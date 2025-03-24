using FluentValidation;
using FluentValidation.Results;
using SharedKernel;
using StockMarketSimulator.Api.Infrastructure.Helpers;
using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Stocks.Contracts;
using StockMarketSimulator.Api.Modules.Stocks.Domain;
using StockMarketSimulator.Api.Modules.Stocks.Infrastructure;

namespace StockMarketSimulator.Api.Modules.Stocks.Application.GetByTicker;

internal sealed class GetStockTickerQueryHandler : IQueryHandler<GetStockByTickerQuery, StockPriceResponse>
{
    private readonly IStockService _stockService;
    private readonly IValidator<GetStockByTickerQuery> _validator;

    public GetStockTickerQueryHandler(
        IStockService stockService,
        IValidator<GetStockByTickerQuery> validator)
    {
        _stockService = stockService;
        _validator = validator;
    }

    public async Task<Result<StockPriceResponse>> Handle(
        GetStockByTickerQuery query, 
        CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<StockPriceResponse>(ValidationErrorFactory.CreateValidationError(validationResult.Errors));
        }

        StockPriceResponse? result = await _stockService.GetLatestStockPriceAsync(query.Ticker, cancellationToken);

        if (result is null)
        {
            return Result.Failure<StockPriceResponse>(StockErrors.NotFound(query.Ticker));
        }

        return result;
    }
}
