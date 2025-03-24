namespace StockMarketSimulator.Api.Modules.Transactions.Contracts;

internal sealed record BuyTransactionRequest(string Ticker, int Quantity);
