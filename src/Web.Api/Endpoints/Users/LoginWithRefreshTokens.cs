using MediatR;
using Modules.Users.Application.Users.LoginWithRefreshToken;
using Modules.Users.Contracts.Users;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class LoginWithRefreshTokens : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/refresh-tokens", async (
            LoginWithRefreshTokenRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new LoginUserWithRefreshTokenCommand(request.RefreshToken);

            Result<TokenResponse> result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Users)
        .RequireFeature(FeatureFlags.UseV1UsersApi);
    }
}
