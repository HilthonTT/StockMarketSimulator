using SharedKernel;
using StockMarketSimulator.Api.Infrastructure.Database;
using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Transactions.Contracts;
using StockMarketSimulator.Api.Modules.Transactions.Domain;
using StockMarketSimulator.Api.Modules.Users.Domain;
using StockMarketSimulator.Api.Modules.Users.Infrastructure;

namespace StockMarketSimulator.Api.Modules.Transactions.Application.GetByUserId;

internal sealed class GetTransactionByUserIdQueryHandler : IQueryHandler<GetTransactionsByUserIdQuery, List<TransactionResponse>>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUserContext _userContext;

    public GetTransactionByUserIdQueryHandler(
        IDbConnectionFactory dbConnectionFactory,
        ITransactionRepository transactionRepository,
        IUserContext userContext)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _transactionRepository = transactionRepository;
        _userContext = userContext;
    }

    public async Task<Result<List<TransactionResponse>>> Handle(
        GetTransactionsByUserIdQuery query, 
        CancellationToken cancellationToken = default)
    {
        if (query.UserId != _userContext.UserId)
        {
            return Result.Failure<List<TransactionResponse>>(UserErrors.Unauthorized);
        }

        await using var connection = await _dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        List<Transaction> transactions = await _transactionRepository.GetByUserIdAsync(
            connection,
            query.UserId,
            cancellationToken: cancellationToken);

        List<TransactionResponse> response = transactions
            .Select(transaction => new TransactionResponse
            {
                Id = transaction.Id,
                UserId = transaction.UserId,
                Ticker = transaction.Ticker,
                LimitPrice = transaction.LimitPrice,
                Type = transaction.Type,
                Quantity = transaction.Quantity,
                CreatedOnUtc = transaction.CreatedOnUtc,
            }).ToList();

        return response;
    }
}
