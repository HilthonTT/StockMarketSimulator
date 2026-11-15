using Application.Abstractions.Authentication;
using Application.Abstractions.Storage;
using Modules.Users.Domain.Enums;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Files;

internal sealed class Upload : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("files/upload", async (
            IFormFile file,
            IBlobService blobService,
            IUserContext userContext,
            CancellationToken cancellationToken = default) =>
        {
            if (!await ImageUploads.IsValidAsync(file, cancellationToken))
            {
                return Results.BadRequest(ImageUploads.InvalidImageMessage);
            }

            using Stream stream = file.OpenReadStream();

            Guid fileId = await blobService.UploadAsync(stream, file.ContentType, userContext.UserId, cancellationToken);

            return Results.Ok(fileId);
        })
        .DisableAntiforgery()
        .WithTags(Tags.Files)
        .HasPermission(Permission.Read, Permission.Write)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter)
        .RequireFeature(FeatureFlags.UseV1FilesApi);
    }
}
