﻿using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Modules.Stocks.Application.Abstractions.Realtime;
using Modules.Stocks.Contracts.Stocks;
using Modules.Stocks.Infrastructure.Realtime.Options;
using Quartz;
using SharedKernel;

namespace Modules.Stocks.Infrastructure.Realtime;

[DisallowConcurrentExecution]
public sealed class StocksFeedUpdater(
    IActiveTickerManager activeTickerManager,
    IServiceScopeFactory serviceScopeFactory,
    IHubContext<StocksFeedHub, IStocksUpdateClient> hubContext,
    IOptions<StockUpdateOptions> options,
    ILogger<StocksFeedUpdater> logger) : IJob
{
    public const string Name = nameof(StocksFeedUpdater);

    private static readonly Random _random = new();
    private readonly StockUpdateOptions _options = options.Value;

    public Task Execute(IJobExecutionContext context)
    {
        return UpdateStockPricesAsync(context.CancellationToken);
    }

    private async Task UpdateStockPricesAsync(CancellationToken cancellationToken = default)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();
        IStockService stockService = scope.ServiceProvider.GetRequiredService<IStockService>();

        foreach (string ticker in activeTickerManager.GetAllTickers())
        {
            Option<StockPriceResponse> optionCurrentPrice = await stockService.GetLatestStockPriceAsync(ticker, cancellationToken);
            if (!optionCurrentPrice.IsSome)
            {
                continue;
            }

            StockPriceResponse currentPrice = optionCurrentPrice.ValueOrThrow();

            decimal newPrice = CalculateNewPrice(currentPrice);

            var update = new StockPriceUpdate(ticker, newPrice);

            await hubContext.Clients.Group(ticker).ReceiveStockPriceUpdate(update, cancellationToken);

            logger.LogInformation("Updated {Ticker} price to {Price}", ticker, newPrice);
        }
    }

    private decimal CalculateNewPrice(StockPriceResponse currentPrice)
    {
        double dt = 1.0 / 252; // Daily step assuming 252 trading days per year
        double mu = 0.0005; // Expected return (adjust this as needed)
        double sigma = _options.Volatility; // Volatility factor

        double randomShock = sigma * Math.Sqrt(dt) * _random.NextDouble();
        double percentageChange = mu * dt + randomShock;

        decimal newPrice = currentPrice.Price * (decimal)(1 + percentageChange);

        return Math.Round(newPrice, 2);
    }
}
