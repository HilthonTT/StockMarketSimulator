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

        IQueryable<TransactionResponse> transactionResponsesQuery = transactionQuery
            .Where(t => t.UserId == request.UserId)
            .OrderByDescending(t => t.CreatedOnUtc)
            .Select(t => new TransactionResponse(
                t.Id,
                t.UserId,
                t.Ticker,
                t.LimitPrice,
                t.Type,
                t.Quantity,
                t.CreatedOnUtc,
                t.ModifiedOnUtc));

        PagedList<TransactionResponse> response = await PagedList<TransactionResponse>.CreateAsync(
            transactionResponsesQuery,
            request.Page,
            request.PageSize,
            cancellationToken);

        return response;
    }
}
