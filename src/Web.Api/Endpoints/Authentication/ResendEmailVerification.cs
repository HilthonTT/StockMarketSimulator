using Application.Abstractions.Messaging;
using Modules.Users.Application.Authentication.ResendEmailVerification;
using Modules.Users.Contracts.Users;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Authentication;

internal sealed class ResendEmailVerification : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("authentication/resend-email-verification", async (
            ResendEmailVerificationRequest request,
            ICommandHandler<ResendEmailVerificationCommand> handler,
            CancellationToken cancellationToken = default) =>
        {
            return await Result.Create(request, GeneralErrors.UnprocessableRequest)
                .Map(request => new ResendEmailVerificationCommand(request.Email))
                .Bind(command => handler.Handle(command, cancellationToken))
                .Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Authentication)
        .RequireFeature(FeatureFlags.UseV1UsersApi)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter);
    }
}
