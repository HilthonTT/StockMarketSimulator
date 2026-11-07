using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Api.FunctionalTests.Abstractions;
using Api.FunctionalTests.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Budgeting.Contracts.Positions;
using Modules.Budgeting.Contracts.Transactions;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Errors;
using Modules.Budgeting.Domain.ValueObjects;
using Modules.Budgeting.Infrastructure.Database;
using Modules.Stocks.Contracts.Stocks;
using Modules.Stocks.Domain.Entities;
using Modules.Stocks.Infrastructure.Database;

namespace Api.FunctionalTests.Budgeting;

public sealed class PositionsTests : BaseFunctionalTest
{
    public PositionsTests(FunctionalTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task BuyAndSell_Should_MaintainPosition_WithAverageCost()
    {
        // Arrange
        (Guid userId, var tokens) = await RegisterAndLoginAsync();
        await CreateBudgetAsync(userId);
        string ticker = await CreateStockAsync(100m);
        HttpClient client = CreateAuthenticatedClient(tokens.AccessToken);

        // Act + Assert: 3 @ 100
        await TradeAsync(client, "buy", userId, ticker.ToLowerInvariant(), 3);
        await AssertSinglePositionAsync(client, userId, ticker, quantity: 3, averageCost: 100m, totalCost: 300m);

        // 1 @ 120 -> average (300 + 120) / 4 = 105
        await SetStockPriceAsync(ticker, 120m);
        await TradeAsync(client, "buy", userId, ticker, 1);
        await AssertSinglePositionAsync(client, userId, ticker, quantity: 4, averageCost: 105m, totalCost: 420m);

        // Selling keeps the average cost and removes the sold shares' cost basis.
        await TradeAsync(client, "sell", userId, ticker, 2);
        await AssertSinglePositionAsync(client, userId, ticker, quantity: 2, averageCost: 105m, totalCost: 210m);

        PurchasedStockTickersResponse? purchased =
            await client.GetFromJsonAsync<PurchasedStockTickersResponse>($"api/v1/users/{userId}/purchased-stock-tickers");
        Assert.NotNull(purchased);
        Assert.Contains(ticker, purchased.Tickers);
    }

    [Fact]
    public async Task Sell_Should_ReturnBadRequest_WhenSellingMoreThanHeld()
    {
        // Arrange
        (Guid userId, var tokens) = await RegisterAndLoginAsync();
        await CreateBudgetAsync(userId);
        string ticker = await CreateStockAsync(10m);
        HttpClient client = CreateAuthenticatedClient(tokens.AccessToken);

        await TradeAsync(client, "buy", userId, ticker, 2);

        // Act
        HttpResponseMessage response = await PostTradeAsync(client, "sell", userId, ticker, 3);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.GetProblemDetails();
        Assert.Equal(TransactionErrors.InsufficientStock.Code, problem.Title);

        await AssertSinglePositionAsync(client, userId, ticker, quantity: 2, averageCost: 10m, totalCost: 20m);
    }

    [Fact]
    public async Task SellingEverything_Should_RemovePositionFromListAndPurchasedTickers()
    {
        // Arrange
        (Guid userId, var tokens) = await RegisterAndLoginAsync();
        await CreateBudgetAsync(userId);
        string ticker = await CreateStockAsync(50m);
        HttpClient client = CreateAuthenticatedClient(tokens.AccessToken);

        await TradeAsync(client, "buy", userId, ticker, 2);

        // Act
        await TradeAsync(client, "sell", userId, ticker, 2);

        // Assert
        List<PositionResponse>? positions =
            await client.GetFromJsonAsync<List<PositionResponse>>($"api/v1/users/{userId}/positions");
        Assert.NotNull(positions);
        Assert.Empty(positions);

        PurchasedStockTickersResponse? purchased =
            await client.GetFromJsonAsync<PurchasedStockTickersResponse>($"api/v1/users/{userId}/purchased-stock-tickers");
        Assert.NotNull(purchased);
        Assert.DoesNotContain(ticker, purchased.Tickers);
    }

    [Fact]
    public async Task GetPositions_Should_ReturnForbidden_ForAnotherUser()
    {
        // Arrange
        (_, var tokens) = await RegisterAndLoginAsync();
        (Guid otherUserId, _) = await RegisterAndLoginAsync();
        HttpClient client = CreateAuthenticatedClient(tokens.AccessToken);

        // Act
        HttpResponseMessage response = await client.GetAsync($"api/v1/users/{otherUserId}/positions");

        // Assert
        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task GetPositions_Should_ReturnUnauthorized_WithoutToken()
    {
        HttpResponseMessage response = await Client.GetAsync($"api/v1/users/{Guid.NewGuid()}/positions");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    private static async Task AssertSinglePositionAsync(
        HttpClient client,
        Guid userId,
        string ticker,
        int quantity,
        decimal averageCost,
        decimal totalCost)
    {
        HttpResponseMessage response = await client.GetAsync($"api/v1/users/{userId}/positions");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // The web UI relies on this exact camelCase shape.
        using JsonDocument document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        JsonElement position = Assert.Single(document.RootElement.EnumerateArray());

        Assert.Equal(ticker, position.GetProperty("ticker").GetString());
        Assert.Equal(quantity, position.GetProperty("quantity").GetInt32());
        Assert.Equal(averageCost, position.GetProperty("averageCost").GetDecimal());
        Assert.Equal(totalCost, position.GetProperty("totalCost").GetDecimal());
    }

    private static async Task TradeAsync(HttpClient client, string side, Guid userId, string ticker, int quantity)
    {
        HttpResponseMessage response = await PostTradeAsync(client, side, userId, ticker, quantity);

        Assert.True(
            response.IsSuccessStatusCode,
            $"{side} failed with {(int)response.StatusCode}: {await response.Content.ReadAsStringAsync()}");
    }

    private static Task<HttpResponseMessage> PostTradeAsync(
        HttpClient client,
        string side,
        Guid userId,
        string ticker,
        int quantity)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, $"api/v1/transactions/{side}")
        {
            Content = side == "buy"
                ? JsonContent.Create(new BuyTransactionRequest(userId, ticker, quantity))
                : JsonContent.Create(new SellTransactionRequest(userId, ticker, quantity))
        };
        request.Headers.Add("Idempotence-Key", Guid.NewGuid().ToString());

        return client.SendAsync(request);
    }

    private async Task CreateBudgetAsync(Guid userId)
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        BudgetingDbContext dbContext = scope.ServiceProvider.GetRequiredService<BudgetingDbContext>();

        dbContext.Budgets.Add(Budget.Create(userId, Currency.Usd));
        await dbContext.SaveChangesAsync();
    }

    private async Task<string> CreateStockAsync(decimal price)
    {
        string ticker = "T" + Guid.NewGuid().ToString("N")[..6].ToUpperInvariant();

        using IServiceScope scope = Factory.Services.CreateScope();
        StocksDbContext dbContext = scope.ServiceProvider.GetRequiredService<StocksDbContext>();

        dbContext.Stocks.Add(Stock.Create(ticker, price));
        await dbContext.SaveChangesAsync();

        return ticker;
    }

    private async Task SetStockPriceAsync(string ticker, decimal price)
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        StocksDbContext dbContext = scope.ServiceProvider.GetRequiredService<StocksDbContext>();

        await dbContext.Database.ExecuteSqlInterpolatedAsync(
            $"UPDATE stocks.stocks SET price = {price} WHERE ticker = {ticker}");
    }
}
