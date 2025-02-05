using SharedKernel;
using StockMarketSimulator.Api.Endpoints;
using StockMarketSimulator.Api.Extensions;
using StockMarketSimulator.Api.Infrastructure;
using StockMarketSimulator.Api.Modules.Stocks.Application.GetByTicker;
using StockMarketSimulator.Api.Modules.Stocks.Contracts;

namespace StockMarketSimulator.Api.Modules.Stocks.Presentation;

internal sealed class StockEndpoints : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("/stocks/{ticker}", async (
            string ticker,
            GetStockTickerQueryHandler useCase,
            CancellationToken cancellationToken) =>
        {
            var query = new GetStockByTickerQuery(ticker);

            Result<StockPriceResponse> result = await useCase.Handle(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithName("GetLatestStockPrice")
        .WithOpenApi()
        .WithTags(Tags.Stocks);
    }
}
