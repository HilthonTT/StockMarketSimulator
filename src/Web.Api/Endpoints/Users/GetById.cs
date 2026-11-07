using Application.Abstractions.Authentication;
using Application.Abstractions.Messaging;
using Modules.Users.Application.Users.GetById;
using Modules.Users.Domain.Enums;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class GetById : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/{userId:guid}", async (
            Guid userId,
            ISender sender,
            IUserContext userContext,
            CancellationToken cancellationToken = default) =>
        {
            return await Result.Success(new GetUserByIdQuery(userId))
               .Bind(query => sender.Send(query, cancellationToken))
               // The query result is cached and shared between viewers, so the email address is
               // redacted here (after the cache) for anyone other than the profile owner.
               .Match(
                   user => Results.Ok(user.Id == userContext.UserId ? user : user with { Email = string.Empty }),
                   CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .HasPermission(Permission.Write, Permission.Read)
        .RequireFeature(FeatureFlags.UseV1UsersApi)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter);
    }
}
