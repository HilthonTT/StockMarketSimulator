using MediatR;
using Modules.Stocks.Application.Shorten.Get;
using Modules.Stocks.Contracts.Shorten;
using SharedKernel;
using Web.Api.Extensions;
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
            var query = new GetShortenUrlQuery(shortCode);

            Result<ShortenUrlResponse> result = await sender.Send(query, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Shorten);
    }
}
