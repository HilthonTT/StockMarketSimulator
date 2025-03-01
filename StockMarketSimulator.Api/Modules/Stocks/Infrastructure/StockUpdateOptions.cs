﻿namespace StockMarketSimulator.Api.Modules.Stocks.Infrastructure;

internal sealed class StockUpdateOptions
{
    public TimeSpan UpdateInterval { get; set; } = TimeSpan.FromSeconds(5);

    public double MaxPercentageChange { get; set; } = 0.02; // 2%
}
