using Application.Abstractions.Messaging;
using Modules.Users.Application.Authentication.VerifyEmail;
using Modules.Users.Application.Users;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Authentication;

internal sealed class VerifyEmail : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("authentication/verify-email", async (
            Guid token,
            ICommandHandler<VerifyEmailCommand> handler,
            CancellationToken cancellationToken = default) =>
        {
            return await Result.Success(new VerifyEmailCommand(token))
                .Bind(command => handler.Handle(command, cancellationToken))
                .Match(Results.NoContent, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Authentication)
        .WithName(UserEndpoints.VerifyEmail)
        .RequireFeature(FeatureFlags.UseV1UsersApi)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter);
    }
}
