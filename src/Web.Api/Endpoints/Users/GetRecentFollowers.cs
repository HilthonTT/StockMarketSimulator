using Application.Abstractions.Messaging;
using Modules.Users.Application.Followers.GetRecentFollowers;
using Modules.Users.Domain.Enums;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class GetRecentFollowers : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/{userId:guid}/followers/recent", async (
            Guid userId,
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            return await Result.Success(new GetRecentFollowersQuery(userId))
                .Bind(query => sender.Send(query, cancellationToken))
                .Match(Results.Ok, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Users)
        .HasPermission(Permission.Write, Permission.Read)
        .RequireFeature(FeatureFlags.UseV1UsersApi)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter);
    }
}
