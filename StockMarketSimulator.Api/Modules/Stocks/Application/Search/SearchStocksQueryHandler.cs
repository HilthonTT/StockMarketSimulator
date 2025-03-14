using FluentValidation;
using FluentValidation.Results;
using SharedKernel;
using StockMarketSimulator.Api.Infrastructure.Helpers;
using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Stocks.Domain;
using StockMarketSimulator.Api.Modules.Stocks.Infrastructure;

namespace StockMarketSimulator.Api.Modules.Stocks.Application.Search;

internal sealed class SearchStocksQueryHandler : IQueryHandler<SearchStocksQuery, List<Match>>
{
    private readonly IStocksClient _stocksClient;
    private readonly IValidator<SearchStocksQuery> _validator;

    public SearchStocksQueryHandler(
        IStocksClient stocksClient,
        IValidator<SearchStocksQuery> validator)
    {
        _stocksClient = stocksClient;
        _validator = validator;
    }

    public async Task<Result<List<Match>>> Handle(SearchStocksQuery query, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(query, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<List<Match>>(ValidationErrorFactory.CreateValidationError(validationResult.Errors));
        }

        List<Match> result = await _stocksClient.SearchTickerAsync(query.SearchTerm, cancellationToken);

        return result;
    }
}
