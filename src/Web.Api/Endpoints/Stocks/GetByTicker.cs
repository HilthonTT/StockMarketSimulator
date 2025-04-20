using MediatR;
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
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            var query = new GetStockByTickerQuery(ticker);

            Result<StockPriceResponse> result = await sender.Send(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Stocks)
        .HasPermission(Permission.Read)
        .RequireRateLimiting("token")
        .RequireFeature(FeatureFlags.UseV1StocksApi);
    }
}
