using Application.Abstractions.Storage;
using Modules.Users.Domain.Enums;
using Web.Api.Extensions;
using Web.Api.Features;

namespace Web.Api.Endpoints.Files;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("files/{fileId:guid}", async (
            Guid fileId,
            IBlobService blobService,
            CancellationToken cancellationToken = default) =>
        {
            FileResponse fileResponse = await blobService.DownloadAsync(fileId, cancellationToken);

            return Results.File(fileResponse.Stream, fileResponse.ContentType);
        })
        .WithOpenApi()
        .WithTags(Tags.Files)
        .HasPermission(Permission.Read)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter)
        .RequireFeature(FeatureFlags.UseV1FilesApi);
    }
}
