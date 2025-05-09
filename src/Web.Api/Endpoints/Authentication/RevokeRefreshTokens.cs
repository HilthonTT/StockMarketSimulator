using MediatR;
using Modules.Users.Application.Authentication.RevokeRefreshTokens;
using Modules.Users.Domain.Enums;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Authentication;

internal sealed class RevokeRefreshTokens : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("authentication/{userId:guid}/refresh-tokens", async (
            Guid userId,
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            return await Result.Success(new RevokeRefreshTokensCommand(userId))
                .Bind(command => sender.Send(command, cancellationToken))
                .Match(Results.NoContent, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Authentication)
        .HasPermission(Permission.Write, Permission.Read)
        .RequireFeature(FeatureFlags.UseV1UsersApi)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter);
    }
}
