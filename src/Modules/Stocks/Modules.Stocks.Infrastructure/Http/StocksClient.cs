using System.Globalization;
using Application.Abstractions.Caching;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Modules.Stocks.Application.Abstractions.Http;
using Modules.Stocks.Contracts.AlphaVantage;
using Modules.Stocks.Contracts.Stocks;
using Newtonsoft.Json;
using SharedKernel;

namespace Modules.Stocks.Infrastructure.Http;

internal sealed class StocksClient(
    HttpClient httpClient,
    IConfiguration configuration,
    ICacheService cacheService,
    ILogger<StocksClient> logger) : IStocksClient
{
    public async Task<StockPriceResponse?> GetDataForTickerAsync(string ticker, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Getting stock price information for {Ticker}", ticker);

        string cacheKey = $"stocks-{ticker}";

        StockPriceResponse? stockPriceResponse = await cacheService.GetAsync<StockPriceResponse>(
            cacheKey,
            cancellationToken);

        if (stockPriceResponse is null)
        {
            stockPriceResponse = await GetStockPriceAsync(ticker, cancellationToken);

            await cacheService.SetAsync(cacheKey, stockPriceResponse, TimeSpan.FromMinutes(5), cancellationToken);
        }

        if (stockPriceResponse is null)
        {
            logger.LogWarning("Failed to get stock price information for {Ticker}", ticker);
        }
        else
        {
            logger.LogInformation("Completed getting stock price information for {Ticker}, {@Stock}", ticker, stockPriceResponse);
        }

        return stockPriceResponse;
    }

    public async Task<AlphaVantageSearchData?> SearchTickerAsync(string searchTerm, CancellationToken cancellationToken = default)
    {
        logger.LogInformation("Starting stock search for '{SearchTerm}'", searchTerm);

        string cacheKey = $"stocks-search-{searchTerm}";

        AlphaVantageSearchData? alphaVantageSearchData = await cacheService.GetAsync<AlphaVantageSearchData>(
            cacheKey,
            cancellationToken);

        if (alphaVantageSearchData is null)
        {
            alphaVantageSearchData = await SearchStocksAsync(searchTerm, cancellationToken);

            await cacheService.SetAsync(cacheKey, alphaVantageSearchData, TimeSpan.FromMinutes(30), cancellationToken);
        }

        if (alphaVantageSearchData is null)
        {
            logger.LogWarning("Failed to search stock information for {SearchTerm}", searchTerm);
        }
        else
        {
            logger.LogInformation("Completed searching stock information for '{SearchTerm}'", searchTerm);
        }

        return alphaVantageSearchData;
    }

    private async Task<StockPriceResponse?> GetStockPriceAsync(string ticker, CancellationToken cancellationToken = default)
    {
        string? apiKey = configuration["Stocks:ApiKey"];
        Ensure.NotNullOrEmpty(apiKey, nameof(apiKey));

        string queryString =
            $"?function=TIME_SERIES_INTRADAY&symbol={Uri.EscapeDataString(ticker)}&interval=15min&apikey={Uri.EscapeDataString(apiKey)}";

        string tickerDataString = await httpClient.GetStringAsync(queryString, cancellationToken);

        AlphaVantageData? tickerData = JsonConvert.DeserializeObject<AlphaVantageData>(tickerDataString);

        // Keys are "yyyy-MM-dd HH:mm:ss" timestamps, so the lexicographically largest key is the latest
        // bar; don't rely on JSON/dictionary ordering.
        AlphaVantageTimeSeriesEntry? lastPrice = tickerData?.TimeSeries
            .OrderByDescending(entry => entry.Key, StringComparer.Ordinal)
            .Select(entry => entry.Value)
            .FirstOrDefault();

        if (lastPrice is null)
        {
            return null;
        }

        // The latest traded price is the bar's close (the high overstated every price).
        return new StockPriceResponse(ticker, decimal.Parse(lastPrice.Close, CultureInfo.InvariantCulture));
    }

    private async Task<AlphaVantageSearchData?> SearchStocksAsync(
        string searchTerm,
        CancellationToken cancellationToken = default)
    {
        string? apiKey = configuration["Stocks:ApiKey"];
        Ensure.NotNullOrEmpty(apiKey, nameof(apiKey));

        string queryString =
            $"?function=SYMBOL_SEARCH&keywords={Uri.EscapeDataString(searchTerm)}&apikey={Uri.EscapeDataString(apiKey)}";

        string matchesDataString = await httpClient.GetStringAsync(queryString, cancellationToken);

        AlphaVantageSearchData? searchData = JsonConvert.DeserializeObject<AlphaVantageSearchData>(matchesDataString);

        return searchData;
    }
}
