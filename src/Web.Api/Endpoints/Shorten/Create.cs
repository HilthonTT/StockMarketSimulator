using Application.Abstractions.Messaging;
using Modules.Stocks.Application.Shorten.Create;
using Modules.Stocks.Contracts.Shorten;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Shorten;

internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("shorten", async (
            ICommandHandler<CreateShortenUrlCommand, ShortenUrlResponse> handler,
            string url,
            CancellationToken cancellationToken) =>
        {
            return await Result.Success(new CreateShortenUrlCommand(url))
                .Bind(command => handler.Handle(command, cancellationToken))
                .Match(Results.Ok, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Shorten)
        .RequireFeature(FeatureFlags.UseV1ShortenApi)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter);
    }
}
