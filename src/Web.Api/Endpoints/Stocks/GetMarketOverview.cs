using Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Mvc;
using Modules.Stocks.Application.Stocks.GetMarketOverview;
using Modules.Users.Domain.Enums;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Stocks;

internal sealed class GetMarketOverview : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("stocks/market", async (

            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            return await Result.Success(new GetMarketOverviewQuery())
                .Bind(query => sender.Send(query, cancellationToken))
                .Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Stocks)
        .HasPermission(Permission.Read)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter)
        .RequireFeature(FeatureFlags.UseV1StocksApi);
    }
}
