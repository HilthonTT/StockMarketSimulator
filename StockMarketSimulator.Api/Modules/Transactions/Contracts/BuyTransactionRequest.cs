namespace StockMarketSimulator.Api.Modules.Transactions.Contracts;

internal sealed record BuyTransactionRequest(string Ticker, decimal LimitPrice, int Quantity);
