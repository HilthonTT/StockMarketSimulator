using MediatR;
using Modules.Users.Application.Authentication.LoginWithRefreshToken;
using Modules.Users.Contracts.Users;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Authentication;

internal sealed class LoginWithRefreshTokens : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("authentication/refresh-tokens", async (
            LoginWithRefreshTokenRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            return await Result.Create(request, GeneralErrors.UnprocessableRequest)
                .Map(request => new LoginUserWithRefreshTokenCommand(request.RefreshToken))
                .Bind(command => sender.Send(command, cancellationToken))
                .Match(Results.Ok, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Authentication)
        .RequireFeature(FeatureFlags.UseV1UsersApi)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter);
    }
}
