using Application.Abstractions.Messaging;
using Contracts.Common;
using Modules.Budgeting.Contracts.Transactions;

namespace Modules.Budgeting.Application.Transactions.GetByUserId;

public sealed record GetTransactionsByUserIdQuery(
    Guid UserId, 
    Guid? Cursor, 
    string? SearchTerm,
    int PageSize,
    DateTime? StartDate,
    DateTime? EndDate) : IQuery<CursorResponse<List<TransactionResponse>>>;
