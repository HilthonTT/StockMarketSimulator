using SharedKernel;

namespace StockMarketSimulator.Api.Modules.Stocks.Domain;

internal static class StockErrors
{
    public static Error NotFound(string ticker) => Error.NotFound(
        "Stocks.NotFound",
        $"No stock data available for ticker: {ticker}");
}
