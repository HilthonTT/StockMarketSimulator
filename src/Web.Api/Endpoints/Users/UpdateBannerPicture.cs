using MediatR;
using Modules.Users.Application.Users.UpdateBannerPicture;
using Modules.Users.Domain.Enums;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class UpdateBannerPicture : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("users/{userId:guid}/banner-image", async (
            Guid userId,
            IFormFile file,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                return Results.BadRequest("Only image files are allowed.");
            }

            return await Result.Success(new UpdateUserBannerPictureCommand(userId, file))
               .Bind(query => sender.Send(query, cancellationToken))
               .Match(Results.NoContent, CustomResults.Problem);
        })
        .WithOpenApi()
        .DisableAntiforgery()
        .HasPermission(Permission.Read, Permission.Write)
        .WithTags(Tags.Users)
        .RequireFeature(FeatureFlags.UseV1UsersApi)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter);
    }
}
