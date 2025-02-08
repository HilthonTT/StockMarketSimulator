using FluentValidation;
using FluentValidation.Results;
using SharedKernel;
using StockMarketSimulator.Api.Infrastructure.Database;
using StockMarketSimulator.Api.Infrastructure.Helpers;
using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Stocks.Api;
using StockMarketSimulator.Api.Modules.Transactions.Domain;
using StockMarketSimulator.Api.Modules.Users.Infrastructure;

namespace StockMarketSimulator.Api.Modules.Transactions.Application.Create;

internal sealed class CreateTransactionCommandHandler : ICommandHandler<CreateTransactionCommand, Guid>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUserContext _userContext;
    private readonly IStocksApi _stocksApi;
    private readonly IValidator<CreateTransactionCommand> _validator;

    public CreateTransactionCommandHandler(
        IDbConnectionFactory dbConnectionFactory, 
        ITransactionRepository transactionRepository,
        IUserContext userContext,
        IStocksApi stocksApi,
        IValidator<CreateTransactionCommand> validator)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _transactionRepository = transactionRepository;
        _userContext = userContext;
        _stocksApi = stocksApi;
        _validator = validator;
    }

    public async Task<Result<Guid>> Handle(
    CreateTransactionCommand command,
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

        if (command.Type == TransactionType.Sell)
        {
            List<Transaction> userTransactions = await _transactionRepository.GetByUserIdAsync(
                connection, _userContext.UserId, cancellationToken: cancellationToken);

            int totalOwned = userTransactions
                .Where(t => t.Ticker == command.Ticker)
                .Sum(t => t.Type == TransactionType.Buy ? t.Quantity : -t.Quantity); // Deduct sold stocks

            if (totalOwned < command.Quantity)
            {
                return Result.Failure<Guid>(TransactionErrors.InsufficientStock);
            }
        }

        var transaction = new Transaction
        {
            Id = Guid.NewGuid(),
            UserId = _userContext.UserId,
            Ticker = command.Ticker,
            LimitPrice = command.LimitPrice,
            Quantity = command.Quantity,
            Type = command.Type,
            CreatedOnUtc = DateTime.UtcNow,
        };

        await _transactionRepository.CreateAsync(connection, transaction, cancellationToken: cancellationToken);

        return transaction.Id;
    }
}
