namespace StockMarketSimulator.Api.Modules.Transactions.Contracts;

internal sealed record SellTransactionRequest(string Ticker, int Quantity);
