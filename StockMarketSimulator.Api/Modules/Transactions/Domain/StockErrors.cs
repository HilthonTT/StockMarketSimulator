using SharedKernel;

namespace StockMarketSimulator.Api.Modules.Transactions.Domain;

internal static class StockErrors
{
    public static Error NotFound(string ticker) => Error.NotFound(
        "Stocks.NotFound",
        $"No stock data available for ticker: {ticker}");
}
