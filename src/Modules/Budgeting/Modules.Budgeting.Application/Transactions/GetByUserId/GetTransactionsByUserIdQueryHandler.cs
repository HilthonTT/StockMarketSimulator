using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Contracts.Common;
using Microsoft.EntityFrameworkCore;
using Modules.Budgeting.Application.Abstractions.Data;
using Modules.Budgeting.Contracts.Transactions;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Errors;
using SharedKernel;

namespace Modules.Budgeting.Application.Transactions.GetByUserId;

internal sealed class GetTransactionsByUserIdQueryHandler(
    IUserContext userContext,
    IDbContext dbContext) : IQueryHandler<GetTransactionsByUserIdQuery, CursorResponse<List<TransactionResponse>>>
{
    public async Task<Result<CursorResponse<List<TransactionResponse>>>> Handle(
        GetTransactionsByUserIdQuery request, 
        CancellationToken cancellationToken)
    {
        if (request.UserId != userContext.UserId)
        {
            return Result.Failure<CursorResponse<List<TransactionResponse>>>(UserErrors.Unauthorized);
        }

        IQueryable<Transaction> transactionQuery = dbContext.Transactions.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            string searchTerm = request.SearchTerm.ToLower();

            transactionQuery = transactionQuery.Where(t =>
                EF.Functions.ToTsVector("english", t.Ticker)
                    .Matches(EF.Functions.PhraseToTsQuery("english", searchTerm)));
        }

        if (request.StartDate is not null)
        {
            transactionQuery = transactionQuery.Where(t => t.CreatedOnUtc >= request.StartDate);
        }

        if (request.EndDate is not null)
        {
            transactionQuery = transactionQuery.Where(t => t.CreatedOnUtc <= request.EndDate);
        }

        transactionQuery = transactionQuery
            .Where(t => t.UserId == request.UserId)
            .OrderBy(t => t.Id);

        if (request.Cursor is not null)
        {
            transactionQuery = transactionQuery.Where(t => t.Id > request.Cursor);
        }

        IQueryable<TransactionResponse> transactionResponsesQuery = transactionQuery
            .Take(request.PageSize + 1)
            .OrderBy(t => t.Id)
            .Select(t => new TransactionResponse(
                t.Id,
                t.UserId,
                t.Ticker,
                t.Money.Amount,
                t.Money.Currency.Code,
                t.Type.Id,
                t.Quantity,
                t.CreatedOnUtc,
                t.ModifiedOnUtc));

        List<TransactionResponse> transactions = await transactionResponsesQuery.ToListAsync(cancellationToken);

        bool hasMore = transactions.Count > request.PageSize;
        if (hasMore)
        {
            transactions = [.. transactions.Take(request.PageSize)];
        }

        Guid? cursor = hasMore ? transactions[^1].Id : null;

        return new CursorResponse<List<TransactionResponse>>(cursor, transactions);
    }
}
