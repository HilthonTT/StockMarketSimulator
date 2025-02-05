using Newtonsoft.Json;
using StockMarketSimulator.Api.Infrastructure.Caching;
using StockMarketSimulator.Api.Modules.Stocks.Contracts;
using StockMarketSimulator.Api.Modules.Stocks.Domain;
using System.Globalization;

namespace StockMarketSimulator.Api.Modules.Stocks.Infrastructure;

internal sealed class StocksClient
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;
    private readonly ICacheService _cacheService;
    private readonly ILogger<StocksClient> _logger;

    public StocksClient(
        HttpClient httpClient,
        IConfiguration configuration,
        ICacheService cacheService,
        ILogger<StocksClient> logger)
    {
        _httpClient = httpClient;
        _configuration = configuration;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<StockPriceResponse?> GetDataForTickerAsync(string ticker, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting stock price information for {Ticker}", ticker);

        string cacheKey = $"stocks-{ticker}";

        StockPriceResponse? stockPriceResponse = await _cacheService.GetAsync<StockPriceResponse>(
            cacheKey,
            cancellationToken);

        if (stockPriceResponse is null)
        {
            stockPriceResponse = await GetStockPriceAsync(ticker, cancellationToken);

            await _cacheService.SetAsync(cacheKey, stockPriceResponse, TimeSpan.FromMinutes(5), cancellationToken);
        }

        if (stockPriceResponse is null)
        {
            _logger.LogWarning("Failed to get stock price information for {Ticker}", ticker);
        }
        else
        {
            _logger.LogInformation("Completed getting stock price information for {Ticker}, {@Stock}", ticker, stockPriceResponse);
        }

        return stockPriceResponse;
    }

    private async Task<StockPriceResponse?> GetStockPriceAsync(string ticker, CancellationToken cancellationToken = default)
    {
        string queryString =
            $"?function=TIME_SERIES_INTRADAY&symbol={ticker}&interval=15min&apikey={_configuration["Stocks:ApiKey"]}";

        string tickerDataString = await _httpClient.GetStringAsync(queryString, cancellationToken);

        AlphaVantageData? tickerData = JsonConvert.DeserializeObject<AlphaVantageData>(tickerDataString);

        TimeSeriesEntry? lastPrice = tickerData?.TimeSeries.FirstOrDefault().Value;
        if (lastPrice is null)
        {
            return null;
        }

        return new StockPriceResponse(ticker, decimal.Parse(lastPrice.High, CultureInfo.InvariantCulture));
    }
}
