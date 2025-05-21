using System.Data;
using Application.Abstractions.Authentication;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Modules.Budgeting.Contracts.Transactions;
using Modules.Budgeting.Domain.Errors;
using SharedKernel;

namespace Modules.Budgeting.Application.Transactions.GetTransactionCount;

internal sealed class GetTransactionCountQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    IUserContext userContext) : IQueryHandler<GetTransactionCountQuery, TransactionCountResponse>
{
    public async Task<Result<TransactionCountResponse>> Handle(
        GetTransactionCountQuery request, 
        CancellationToken cancellationToken)
    {
        if (userContext.UserId != request.UserId)
        {
            return Result.Failure<TransactionCountResponse>(UserErrors.Unauthorized);
        }

        using IDbConnection connection = await dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        TransactionCountResponse response = 
            await TransactionQueries.GetTransactionCountAsync(connection, request.UserId, cancellationToken);

        return response;
    }
}
