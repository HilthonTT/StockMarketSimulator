using Application.Abstractions.Storage;
using Modules.Users.Domain.Enums;
using Web.Api.Extensions;
using Web.Api.Features;

namespace Web.Api.Endpoints.Files;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("files/{fileId:guid}", async (
            Guid fileId,
            IBlobService blobService,
            CancellationToken cancellationToken) =>
        {
            await blobService.DeleteAsync(fileId, cancellationToken);

            return Results.NoContent();
        })
        .WithOpenApi()
        .WithTags(Tags.Files)
        .HasPermission(Permission.Write)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter)
        .RequireFeature(FeatureFlags.UseV1FilesApi);
    }
}
