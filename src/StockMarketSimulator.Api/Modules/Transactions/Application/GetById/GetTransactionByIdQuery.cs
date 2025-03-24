using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Transactions.Contracts;

namespace StockMarketSimulator.Api.Modules.Transactions.Application.GetById;

internal sealed record GetTransactionByIdQuery(Guid TransactionId) : IQuery<TransactionResponse>;
