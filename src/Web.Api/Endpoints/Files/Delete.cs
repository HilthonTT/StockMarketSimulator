using Application.Abstractions.Authentication;
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
            IUserContext userContext,
            CancellationToken cancellationToken) =>
        {
            // Only the uploader may delete a file. Files without a recorded owner (profile/banner
            // pictures from before owners were tracked) can only be replaced through the user endpoints.
            Guid? ownerId = await blobService.GetOwnerAsync(fileId, cancellationToken);
            if (ownerId != userContext.UserId)
            {
                return Results.NotFound();
            }

            await blobService.DeleteAsync(fileId, cancellationToken);

            return Results.NoContent();
        })
        .WithTags(Tags.Files)
        .HasPermission(Permission.Write)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter)
        .RequireFeature(FeatureFlags.UseV1FilesApi);
    }
}
