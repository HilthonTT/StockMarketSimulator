using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Transactions.Contracts;

namespace StockMarketSimulator.Api.Modules.Transactions.Application.GetByUserId;

public sealed record GetTransactionsByUserIdQuery(Guid UserId) : IQuery<List<TransactionResponse>>;
