using Application.Abstractions.Messaging;
using Modules.Stocks.Application.Shorten.Get;
using Modules.Stocks.Application.Shorten.GetByTicker;
using Modules.Stocks.Contracts.Shorten;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Shorten;

internal sealed class GetByTicker : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("shorten/ticker/{ticker}", async (
            string ticker,
            IQueryHandler<GetShortenUrlByTickerQuery, ShortenUrlResponse> handler,
            CancellationToken cancellationToken) =>
        {
            return await Result.Success(new GetShortenUrlByTickerQuery(ticker))
               .Bind(query => handler.Handle(query, cancellationToken))
               .Match(Results.Ok, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Shorten)
        .RequireFeature(FeatureFlags.UseV1ShortenApi)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter);
    }
}
