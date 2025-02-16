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

namespace StockMarketSimulator.Api.Modules.Transactions.Application.Buy;

internal sealed class BuyTransactionCommandHandler : ICommandHandler<BuyTransactionCommand, Guid>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUserContext _userContext;
    private readonly IStocksApi _stocksApi;
    private readonly IValidator<BuyTransactionCommand> _validator;
    private readonly IBudgetsApi _budgetsApi;

    public BuyTransactionCommandHandler(
        IDbConnectionFactory dbConnectionFactory, 
        ITransactionRepository transactionRepository,
        IUserContext userContext,
        IStocksApi stocksApi,
        IValidator<BuyTransactionCommand> validator,
        IBudgetsApi budgetsApi)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _transactionRepository = transactionRepository;
        _userContext = userContext;
        _stocksApi = stocksApi;
        _validator = validator;
        _budgetsApi = budgetsApi;
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
        await using var dbTransaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            BudgetApiResponse? budget = await _budgetsApi.GetByUserIdAsync(
           connection,
           _userContext.UserId,
           dbTransaction,
           cancellationToken);

            if (budget is null)
            {
                return Result.Failure<Guid>(BudgetErrors.NotFoundByUserId(_userContext.UserId));
            }

            StockPriceInfo? stockPriceInfo = await _stocksApi.GetLatestPriceAsync(command.Ticker, cancellationToken);
            if (stockPriceInfo is null)
            {
                return Result.Failure<Guid>(StockErrors.NotFound(command.Ticker));
            }

            decimal totalCost = command.Quantity * stockPriceInfo.Price;

            if (budget.BuyingPower < totalCost)
            {
                return Result.Failure<Guid>(TransactionErrors.InsufficientFunds);
            }

            decimal updatedBuyingPower = budget.BuyingPower - totalCost;

            BudgetApiResponse budgetToUpdate = budget with
            {
                BuyingPower = updatedBuyingPower,
            };

            await _budgetsApi.UpdateAsync(connection, budgetToUpdate, dbTransaction, cancellationToken);

            var transaction = Transaction.Create(
                _userContext.UserId,
                command.Ticker,
                stockPriceInfo.Price,
                TransactionType.Buy,
                command.Quantity);

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
