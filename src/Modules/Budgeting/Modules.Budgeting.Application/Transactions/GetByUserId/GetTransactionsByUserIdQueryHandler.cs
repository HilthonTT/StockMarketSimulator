using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Contracts.Common;
using Modules.Budgeting.Application.Abstractions.Data;
using Modules.Budgeting.Contracts.Transactions;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Errors;
using SharedKernel;

namespace Modules.Budgeting.Application.Transactions.GetByUserId;

internal sealed class GetTransactionsByUserIdQueryHandler(
    IDbContext dbContext,
    IUserContext userContext) : IQueryHandler<GetTransactionsByUserIdQuery, PagedList<TransactionResponse>>
{
    public async Task<Result<PagedList<TransactionResponse>>> Handle(
        GetTransactionsByUserIdQuery request, 
        CancellationToken cancellationToken)
    {
        if (request.UserId != userContext.UserId)
        {
            return Result.Failure<PagedList<TransactionResponse>>(UserErrors.Unauthorized);
        }

        IQueryable<Transaction> transactionQuery = dbContext.Transactions.AsQueryable();
        IQueryable<TransactionResponse> transactionResponsesQuery = transactionQuery
            .Select(t => new TransactionResponse(
                t.Id,
                t.UserId,
                t.Ticker,
                t.LimitPrice,
                t.Type,
                t.Quantity));

        PagedList<TransactionResponse> response = await PagedList<TransactionResponse>.CreateAsync(
            transactionResponsesQuery,
            request.Page,
            request.PageSize,
            cancellationToken);

        return response;
    }
}
