using Application.Abstractions.Storage;
using Modules.Users.Domain.Enums;
using Web.Api.Extensions;
using Web.Api.Features;

namespace Web.Api.Endpoints.Files;

internal sealed class Upload : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("files/upload", async (
            IFormFile file, 
            IBlobService blobService,
            CancellationToken cancellationToken = default) =>
        {
            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return Results.BadRequest("Only image files are allowed.");
            }

            using Stream stream = file.OpenReadStream();

            Guid fileId = await blobService.UploadAsync(stream, file.ContentType, cancellationToken);

            return Results.Ok(fileId);
        })
        .WithOpenApi()
        .DisableAntiforgery()
        .WithTags(Tags.Files)
        .HasPermission(Permission.Read, Permission.Write)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter)
        .RequireFeature(FeatureFlags.UseV1FilesApi);
    }
}
