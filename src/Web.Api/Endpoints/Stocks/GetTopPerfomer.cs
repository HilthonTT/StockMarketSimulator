using Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Mvc;
using Modules.Stocks.Application.Stocks.GetTopPerfomer;
using Modules.Stocks.Contracts.Stocks;
using Modules.Users.Domain.Enums;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Stocks;

internal sealed class GetTopPerfomer : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("stocks/top-performer", async (
            [FromHeader] Guid userId,
            IQueryHandler<GetTopPerformerQuery, StockPriceResponse> handler,
            CancellationToken cancellationToken = default) =>
        {
            return await Result.Success(new GetTopPerformerQuery(userId))
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
