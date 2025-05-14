using Application.Abstractions.Messaging;
using Modules.Stocks.Application.Stocks.GetByTicker;
using Modules.Stocks.Contracts.Stocks;
using Modules.Users.Domain.Enums;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Stocks;

internal sealed class GetByTicker : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("stocks/{ticker}", async (
            string ticker,
            IQueryHandler<GetStockByTickerQuery, StockPriceResponse> handler,
            CancellationToken cancellationToken = default) =>
        {
            return await Result.Success(new GetStockByTickerQuery(ticker))
               .Bind(query => handler.Handle(query, cancellationToken))
               .Match(Results.Ok, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Stocks)
        .HasPermission(Permission.Read)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter)
        .RequireFeature(FeatureFlags.UseV1StocksApi);
    }
}
