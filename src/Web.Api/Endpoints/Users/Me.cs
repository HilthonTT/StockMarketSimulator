using Application.Abstractions.Messaging;
using Modules.Users.Application.Users.Me;
using Modules.Users.Contracts.Users;
using Modules.Users.Domain.Enums;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class Me : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/me", async (
            IQueryHandler<GetMeQuery, UserResponse> handler, 
            CancellationToken cancellationToken = default) =>
        {
            return await Result.Success(new GetMeQuery())
                .Bind(query => handler.Handle(query, cancellationToken))
                .Match(Results.Ok, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Users)
        .HasPermission(Permission.Write, Permission.Read)
        .RequireFeature(FeatureFlags.UseV1UsersApi)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter);
    }
}
