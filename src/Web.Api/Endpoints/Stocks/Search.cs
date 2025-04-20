using Contracts.Common;
using MediatR;
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
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            var query = new SearchStocksQuery(searchTerm, page, pageSize);

            Result<PagedList<StockSearchResponse>> result = await sender.Send(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Stocks)
        .HasPermission(Permission.Read)
        .RequireRateLimiting("token")
        .RequireFeature(FeatureFlags.UseV1StocksApi);
    }
}
