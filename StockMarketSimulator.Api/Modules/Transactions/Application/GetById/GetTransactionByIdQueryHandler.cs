using SharedKernel;
using StockMarketSimulator.Api.Infrastructure.Database;
using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Transactions.Contracts;
using StockMarketSimulator.Api.Modules.Transactions.Domain;
using StockMarketSimulator.Api.Modules.Users.Domain;
using StockMarketSimulator.Api.Modules.Users.Infrastructure;

namespace StockMarketSimulator.Api.Modules.Transactions.Application.GetById;

internal sealed class GetTransactionByIdQueryHandler : IQueryHandler<GetTransactionByIdQuery, TransactionResponse>
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly IUserContext _userContext;
    private readonly ITransactionRepository _transactionRepository;

    public GetTransactionByIdQueryHandler(
        IDbConnectionFactory dbConnectionFactory,
        IUserContext userContext,
        ITransactionRepository transactionRepository)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _userContext = userContext;
        _transactionRepository = transactionRepository;
    }

    public async Task<Result<TransactionResponse>> Handle(
        GetTransactionByIdQuery query, 
        CancellationToken cancellationToken = default)
    {
        await using var connection = await _dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        Transaction? transaction = await _transactionRepository.GetByIdAsync(
            connection,
            query.TransactionId,
            cancellationToken: cancellationToken);

        if (transaction is null)
        {
            return Result.Failure<TransactionResponse>(TransactionErrors.NotFound(query.TransactionId));
        }

        if (transaction.UserId != _userContext.UserId)
        {
            return Result.Failure<TransactionResponse>(UserErrors.Unauthorized);
        }

        return new TransactionResponse
        {
            Id = transaction.Id,
            UserId = transaction.UserId,
            LimitPrice = transaction.LimitPrice,
            Type = transaction.Type,
            Quantity = transaction.Quantity,
            CreatedOnUtc = transaction.CreatedOnUtc,
        };
    }
}
