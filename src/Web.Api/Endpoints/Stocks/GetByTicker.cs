using Application.Abstractions.Messaging;
using Modules.Stocks.Application.Stocks.GetByTicker;
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
            return await Result.Success(new GetStockByTickerQuery(ticker))
               .Bind(query => sender.Send(query, cancellationToken))
               .Match(Results.Ok, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Stocks)
        .HasPermission(Permission.Read)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter)
        .RequireFeature(FeatureFlags.UseV1StocksApi);
    }
}
