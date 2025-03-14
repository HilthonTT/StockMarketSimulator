using Api.Tests.Infrastructure;
using StockMarketSimulator.Api.Modules.Stocks.Contracts;
using System.Net.Http.Json;

namespace Api.Tests;

public sealed class StocksEndpointTests : BaseIntegrationTest
{
    public StocksEndpointTests(CustomWebAppFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Get_StockPrice_Returns_Success()
    {
        // Arrange
        string ticker = "AAPL";
        var expectedResponse = new StockPriceResponse(ticker, 145.30M); // Arbitrary price, just for mock

        SetupStockApiMock(ticker, expectedResponse);

        // Act
        HttpResponseMessage response = await Client.GetAsync($"/api/v1/stocks/{ticker}");

        // Assert
        response.EnsureSuccessStatusCode();

        StockPriceResponse? stockPrice = await response.Content.ReadFromJsonAsync<StockPriceResponse>();

        Assert.NotNull(stockPrice);
        Assert.Equal(expectedResponse.Ticker, stockPrice?.Ticker);
        Assert.True(stockPrice?.Price >= 0, "Stock price should be a non-negative value.");
    }
}
