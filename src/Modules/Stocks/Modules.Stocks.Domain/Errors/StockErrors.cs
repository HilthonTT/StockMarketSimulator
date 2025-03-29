using SharedKernel;

namespace Stocks.Domain.Errors;

public static class StockErrors
{
    public static Error NotFound(string ticker) => Error.NotFound(
        "Stocks.NotFound",
        $"No stock data available for ticker: {ticker}");
}
