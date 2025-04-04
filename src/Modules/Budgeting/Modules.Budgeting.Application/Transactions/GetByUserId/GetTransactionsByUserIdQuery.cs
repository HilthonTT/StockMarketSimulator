using Application.Abstractions.Messaging;
using Contracts.Common;
using Modules.Budgeting.Contracts.Transactions;

namespace Modules.Budgeting.Application.Transactions.GetByUserId;

public sealed record GetTransactionsByUserIdQuery(Guid UserId, int Page, int PageSize)
    : IQuery<PagedList<TransactionResponse>>;
