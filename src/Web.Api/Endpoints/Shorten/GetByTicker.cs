using MediatR;
using Modules.Stocks.Application.Shorten.GetByTicker;
using Modules.Stocks.Contracts.Shorten;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Shorten;

internal sealed class GetByTicker : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("shorten/ticker/{ticker}", async (
            string ticker,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var query = new GetShortenUrlByTickerQuery(ticker);

            Result<ShortenUrlResponse> result = await sender.Send(query, cancellationToken);

            return result.Match((result) => Results.Redirect(result.ShortCode), CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Shorten);
    }
}
