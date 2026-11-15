using Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Mvc;
using Modules.Stocks.Application.Stocks.GetMarketNews;
using Modules.Users.Domain.Enums;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Stocks;

internal sealed class GetMarketNews : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("stocks/news", async (
            [FromQuery] int? limit,
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            return await Result.Success(new GetMarketNewsQuery(limit ?? 20))
                .Bind(query => sender.Send(query, cancellationToken))
                .Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Stocks)
        .HasPermission(Permission.Read)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter)
        .RequireFeature(FeatureFlags.UseV1StocksApi);
    }
}
