using Application.Abstractions.Messaging;
using Modules.Stocks.Application.Shorten.Get;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Shorten;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("shorten/{shortCode}", async (
            string shortCode,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            return await Result.Success(new GetShortenUrlQuery(shortCode))
                .Bind(query => sender.Send(query, cancellationToken))
                .Match(Results.Ok, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Shorten)
        .RequireFeature(FeatureFlags.UseV1ShortenApi)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter);
    }
}
