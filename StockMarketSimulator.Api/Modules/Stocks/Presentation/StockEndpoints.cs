﻿using SharedKernel;
using StockMarketSimulator.Api.Endpoints;
using StockMarketSimulator.Api.Extensions;
using StockMarketSimulator.Api.Infrastructure;
using StockMarketSimulator.Api.Infrastructure.Messaging;
using StockMarketSimulator.Api.Modules.Stocks.Application.GetByTicker;
using StockMarketSimulator.Api.Modules.Stocks.Application.Search;
using StockMarketSimulator.Api.Modules.Stocks.Contracts;
using StockMarketSimulator.Api.Modules.Stocks.Domain;

namespace StockMarketSimulator.Api.Modules.Stocks.Presentation;

internal sealed class StockEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/stocks/{ticker}", async (
            string ticker,
            IQueryHandler<GetStockByTickerQuery, StockPriceResponse> useCase,
            CancellationToken cancellationToken) =>
        {
            var query = new GetStockByTickerQuery(ticker);

            Result<StockPriceResponse> result = await useCase.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithName("GetLatestStockPrice")
        .WithOpenApi()
        .WithTags(Tags.Stocks);

        app.MapGet("/stocks/search/{searchTerm}", async (
            string searchTerm,
            IQueryHandler<SearchStocksQuery, List<Match>> useCase,
            CancellationToken cancellationToken) =>
        {
            var query = new SearchStocksQuery(searchTerm);

            Result<List<Match>> result = await useCase.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithName("SearchStockInformation")
        .WithOpenApi()
        .WithTags(Tags.Stocks);
    }
}
