using MediatR;
using Modules.Stocks.Application.Shorten.Create;
using Modules.Stocks.Contracts.Shorten;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Shorten;

internal sealed class Create : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("shorten", async (
            ISender sender,
            string url,
            CancellationToken cancellationToken) =>
        {
            var command = new CreateShortenUrlCommand(url);

            Result<ShortenUrlResponse> result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Shorten);
    }
}
