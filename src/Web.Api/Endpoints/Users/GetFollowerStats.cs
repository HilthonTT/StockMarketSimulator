using Application.Abstractions.Messaging;
using Modules.Users.Application.Followers.GetFollowerStats;
using Modules.Users.Domain.Enums;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class GetFollowerStats : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/{userId:guid}/followers/stats", async (
            Guid userId,
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            return await Result.Success(new GetFollowerStatsQuery(userId))
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
