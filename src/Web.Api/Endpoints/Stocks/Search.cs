using Application.Abstractions.Messaging;
using Contracts.Common;
using Microsoft.AspNetCore.Mvc;
using Modules.Stocks.Application.Stocks.Search;
using Modules.Stocks.Contracts.Stocks;
using Modules.Users.Domain.Enums;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Stocks;

internal sealed class Search : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("stocks/search", async (
            [FromQuery] string? searchTerm,
            [FromQuery] int page,
            [FromQuery] int pageSize,
            IQueryHandler<SearchStocksQuery, PagedList<StockSearchResponse>> handler,
            CancellationToken cancellationToken = default) =>
        {
            return await Result.Success(new SearchStocksQuery(searchTerm, page, pageSize))
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
