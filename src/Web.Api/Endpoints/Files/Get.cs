using Application.Abstractions.Storage;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Files;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("files/{fileId:guid}", async (
            Guid fileId,
            HttpContext httpContext,
            IBlobService blobService,
            CancellationToken cancellationToken = default) =>
        {
            FileResponse? fileResponse = await blobService.DownloadAsync(fileId, cancellationToken);
            if (fileResponse is null)
            {
                return Results.NotFound();
            }

            // Never let the browser reinterpret the bytes (e.g. as HTML or SVG with script).
            httpContext.Response.Headers.XContentTypeOptions = "nosniff";

            // Allow-listed raster images render inline; anything else (e.g. SVGs uploaded before the
            // allow-list existed) is forced to download instead of executing in the API origin.
            return ImageUploads.IsAllowedContentType(fileResponse.ContentType)
                ? Results.File(fileResponse.Stream, fileResponse.ContentType)
                : Results.File(fileResponse.Stream, "application/octet-stream", fileDownloadName: fileId.ToString());
        })
        .WithTags(Tags.Files)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter)
        .RequireFeature(FeatureFlags.UseV1FilesApi);
    }
}
