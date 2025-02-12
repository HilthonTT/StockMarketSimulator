using FluentValidation;
using FluentValidation.Results;
using SharedKernel;
using StockMarketSimulator.Api.Infrastructure.Database;
using StockMarketSimulator.Api.Infrastructure.Helpers;
using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Budgets.Api;
using StockMarketSimulator.Api.Modules.Budgets.Domain;
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
    private readonly IBudgetsApi _budgetsApi;

    public SellTransactionCommandHandler(
        IDbConnectionFactory dbConnectionFactory,
        ITransactionRepository transactionRepository,
        IUserContext userContext,
        IStocksApi stocksApi,
        IValidator<SellTransactionCommand> validator,
        IBudgetsApi budgetsApi)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _transactionRepository = transactionRepository;
        _userContext = userContext;
        _stocksApi = stocksApi;
        _validator = validator;
        _budgetsApi = budgetsApi;
    }

    public async Task<Result<Guid>> Handle(SellTransactionCommand command, CancellationToken cancellationToken = default)
    {
        ValidationResult validationResult = await _validator.ValidateAsync(command, cancellationToken);
        if (!validationResult.IsValid)
        {
            return Result.Failure<Guid>(ValidationErrorFactory.CreateValidationError(validationResult.Errors));
        }

        await using var connection = await _dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);
        await using var dbTransaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            StockPriceInfo? stockPriceInfo = await _stocksApi.GetLatestPriceAsync(command.Ticker, cancellationToken);
            if (stockPriceInfo is null)
            {
                return Result.Failure<Guid>(StockErrors.NotFound(command.Ticker));
            }

            List<Transaction> userTransactions = await _transactionRepository.GetByUserIdAsync(
                connection, _userContext.UserId, dbTransaction, cancellationToken);

            int totalOwned = userTransactions
                .Where(t => t.Ticker == command.Ticker)
                .Sum(t => t.Type == TransactionType.Buy ? t.Quantity : -t.Quantity); // Deduct sold stocks

            if (totalOwned < command.Quantity)
            {
                return Result.Failure<Guid>(TransactionErrors.InsufficientStock);
            }

            decimal totalEarnings = command.Quantity * stockPriceInfo.Price;

            BudgetApiResponse? budget = await _budgetsApi.GetByUserIdAsync(
                connection, 
                _userContext.UserId, 
                dbTransaction, 
                cancellationToken);

            if (budget is null)
            {
                return Result.Failure<Guid>(BudgetErrors.NotFoundByUserId(_userContext.UserId));
            }

            decimal updatedBuyingPower = budget.BuyingPower + totalEarnings;

            budget = budget with
            {
                BuyingPower = updatedBuyingPower,
            };

            await _budgetsApi.UpdateAsync(connection, budget, dbTransaction, cancellationToken);

            var transaction = new Transaction
            {
                Id = Guid.NewGuid(),
                UserId = _userContext.UserId,
                Ticker = command.Ticker,
                LimitPrice = stockPriceInfo.Price,
                Quantity = command.Quantity,
                Type = TransactionType.Sell,
                CreatedOnUtc = DateTime.UtcNow,
            };

            await _transactionRepository.CreateAsync(connection, transaction, dbTransaction, cancellationToken);

            await dbTransaction.CommitAsync(cancellationToken);

            return transaction.Id;
        }
        catch (Exception)
        {
            await dbTransaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}
