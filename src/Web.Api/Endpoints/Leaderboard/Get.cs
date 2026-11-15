using Application.Abstractions.Messaging;
using Microsoft.AspNetCore.Mvc;
using Modules.Users.Domain.Enums;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Idempotency;
using Web.Api.Infrastructure;
using Modules.Users.Application.Leaderboard;

namespace Web.Api.Endpoints.Leaderboard;

internal sealed class Get : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("leaderboard", async (
            [FromQuery] int? limit,
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            return await Result.Success(new GetLeaderboardQuery(limit ?? 25))
                .Bind(query => sender.Send(query, cancellationToken))
                .Match(Results.Ok, CustomResults.Problem);
        })
        .WithTags(Tags.Leaderboard)
        .HasPermission(Permission.Read)
        .RequireFeature(FeatureFlags.UseV1UsersApi)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter);
    }
}
