using FluentValidation;
using FluentValidation.Results;
using SharedKernel;
using StockMarketSimulator.Api.Infrastructure.Database;
using StockMarketSimulator.Api.Infrastructure.Helpers;
using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Stocks.Api;
using StockMarketSimulator.Api.Modules.Transactions.Domain;
using StockMarketSimulator.Api.Modules.Users.Infrastructure;

namespace StockMarketSimulator.Api.Modules.Transactions.Application.Buy;

internal sealed class BuyTransactionCommandHandler : ICommandHandler<BuyTransactionCommand, Guid>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUserContext _userContext;
    private readonly IStocksApi _stocksApi;
    private readonly IValidator<BuyTransactionCommand> _validator;

    public BuyTransactionCommandHandler(
        IDbConnectionFactory dbConnectionFactory, 
        ITransactionRepository transactionRepository,
        IUserContext userContext,
        IStocksApi stocksApi,
        IValidator<BuyTransactionCommand> validator)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _transactionRepository = transactionRepository;
        _userContext = userContext;
        _stocksApi = stocksApi;
        _validator = validator;
    }

    public async Task<Result<Guid>> Handle(
    BuyTransactionCommand command,
    CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<Guid>(ValidationErrorFactory.CreateValidationError(validationResult.Errors));
        }

        await using var connection = await _dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        StockPriceInfo? stockPriceInfo = await _stocksApi.GetLatestPriceAsync(command.Ticker, cancellationToken);
        if (stockPriceInfo is null)
        {
            return Result.Failure<Guid>(StockErrors.NotFound(command.Ticker));
        }

        if (command.LimitPrice > stockPriceInfo.Price)
        {
            return Result.Failure<Guid>(TransactionErrors.LimitPriceExceedsMarketPrice);
        }

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = _userContext.UserId,
            Ticker = command.Ticker,
            LimitPrice = command.LimitPrice,
            Quantity = command.Quantity,
            Type = TransactionType.Buy,
            CreatedOnUtc = DateTime.UtcNow,
        };

        await _transactionRepository.CreateAsync(connection, transaction, cancellationToken: cancellationToken);

        return transaction.Id;
    }
}
