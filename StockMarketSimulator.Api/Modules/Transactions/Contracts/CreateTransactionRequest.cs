using StockMarketSimulator.Api.Modules.Transactions.Domain;

namespace StockMarketSimulator.Api.Modules.Transactions.Contracts;

internal sealed record CreateTransactionRequest(string Ticker, decimal LimitPrice, TransactionType Type, int Quantity);
