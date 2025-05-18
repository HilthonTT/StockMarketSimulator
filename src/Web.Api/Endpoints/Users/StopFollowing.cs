using Application.Abstractions.Messaging;
using Modules.Users.Application.Followers.StopFollowing;
using Modules.Users.Domain.Enums;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class StopFollowing : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("users/{userId}/follow/{followedId}", async (
             Guid userId,
             Guid followedId,
             ISender sender,
             CancellationToken cancellationToken = default) =>
        {
            return await Result.Success(new StopFollowingCommand(userId, followedId))
                .Bind(query => sender.Send(query, cancellationToken))
                .Match(Results.NoContent, CustomResults.Problem);
        })
         .WithOpenApi()
         .WithTags(Tags.Users)
         .HasPermission(Permission.Write, Permission.Read)
         .RequireFeature(FeatureFlags.UseV1UsersApi)
         .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter);
    }
}
