using SharedKernel;

namespace Modules.Stocks.Application.Core.Errors;

internal static class StocksValidationErrors
{
    public static class GetTopPerfomer
    {
        public static readonly Error UserIdIsRequired = Error.Problem(
            "GetTopPerfomer.UserIdIsRequired",
            "The user identifier is required.");
    }

    public static class CreateShortenUrl
    {
        public static readonly Error UrlIsRequired = Error.Problem(
            "CreateShortenUrl.UrlIsRequired",
            "The url is required.");

        public static readonly Error UrlMustBeValidUrl = Error.Problem(
            "CreateShortenUrl.UrlMustBeValidUrl",
            "The url must be a proper valid URI.");
    }

    public static class GetShortenUrlByShortCode
    {
        public static readonly Error ShortCodeIsRequired = Error.Problem(
            "GetShortenUrl.ShortCodeIsRequired",
            "The url is required.");
    }

    public static class GetShortenUrlByTicker
    {
        public static readonly Error TickerIsRequired = Error.Problem(
            "GetShortenUrlByTicker.TickerIsRequired",
            "The ticker SYMBOL is required.");

        public static readonly Error TickerInvalidFormat = Error.Problem(
            "GetShortenUrlByTicker.TickerInvalidFormat",
            "The ticker must be at most 10 characters.");
    }

    public static class GetStockByTicker
    {
        public static readonly Error TickerIsRequired = Error.Problem(
            "GetStockByTicker.TickerIsRequired",
            "The ticker SYMBOL is required.");

        public static readonly Error TickerInvalidFormat = Error.Problem(
            "GetStockByTicker.TickerInvalidFormat",
            "The ticker must be at most 10 characters.");
    }

    public static class SearchStocks
    {
        public static readonly Error PageIsRequired = Error.Problem(
            "SearchStocksQuery.PageIsRequired",
            "The page is required.");

        public static readonly Error PageMustBeGreaterThanZero = Error.Problem(
            "SearchStocksQuery.PageMustBeGreaterThanZero",
            "The page must be greater than 0.");

        public static readonly Error PageSizeIsRequired = Error.Problem(
            "SearchStocksQuery.PageSizeIsRequired",
            "The page size is required.");

        public static readonly Error PageSizeMustBeInRange = Error.Problem(
            "SearchStocksQuery.PageSizeMustBeInRange",
            "The page size must be between 1 and 100.");
    }

    public static class GetPurchasedStockTickers
    {
        public static readonly Error UserIdIsRequired = Error.Problem(
            "GetPurchasedStockTickers.UserIdIsRequired",
            "The user identifier is required.");
    }
}
