using FluentValidation;
using FluentValidation.Results;
using SharedKernel;
using StockMarketSimulator.Api.Infrastructure.Database;
using StockMarketSimulator.Api.Infrastructure.Helpers;
using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Stocks.Api;
using StockMarketSimulator.Api.Modules.Transactions.Domain;
using StockMarketSimulator.Api.Modules.Users.Infrastructure;

namespace StockMarketSimulator.Api.Modules.Transactions.Application.Sell;

internal sealed class SellTransactionCommandHandler : ICommandHandler<SellTransactionCommand, Guid>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUserContext _userContext;
    private readonly IStocksApi _stocksApi;
    private readonly IValidator<SellTransactionCommand> _validator;

    public SellTransactionCommandHandler(
        IDbConnectionFactory dbConnectionFactory,
        ITransactionRepository transactionRepository,
        IUserContext userContext,
        IStocksApi stocksApi,
        IValidator<SellTransactionCommand> validator)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _transactionRepository = transactionRepository;
        _userContext = userContext;
        _stocksApi = stocksApi;
        _validator = validator;
    }

    public async Task<Result<Guid>> Handle(SellTransactionCommand command, CancellationToken cancellationToken = default)
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

        List<Transaction> userTransactions = await _transactionRepository.GetByUserIdAsync(
            connection, _userContext.UserId, cancellationToken: cancellationToken);

        int totalOwned = userTransactions
            .Where(t => t.Ticker == command.Ticker)
            .Sum(t => t.Type == TransactionType.Buy ? t.Quantity : -t.Quantity); // Deduct sold stocks

        if (totalOwned < command.Quantity)
        {
            return Result.Failure<Guid>(TransactionErrors.InsufficientStock);
        }

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = _userContext.UserId,
            Ticker = command.Ticker,
            LimitPrice = stockPriceInfo.Price,
            Quantity = command.Quantity,
            Type = TransactionType.Buy,
            CreatedOnUtc = DateTime.UtcNow,
        };

        await _transactionRepository.CreateAsync(connection, transaction, cancellationToken: cancellationToken);

        return transaction.Id;
    }
}
