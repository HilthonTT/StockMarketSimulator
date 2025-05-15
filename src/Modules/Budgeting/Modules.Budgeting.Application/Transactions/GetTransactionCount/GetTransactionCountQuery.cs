using Application.Abstractions.Messaging;
using Modules.Budgeting.Contracts.Transactions;

namespace Modules.Budgeting.Application.Transactions.GetTransactionCount;

public sealed record GetTransactionCountQuery(Guid UserId) : IQuery<TransactionCountResponse>;
