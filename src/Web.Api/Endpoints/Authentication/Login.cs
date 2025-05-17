using Application.Abstractions.Messaging;
using Modules.Users.Application.Authentication.Login;
using Modules.Users.Contracts.Users;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Authentication;

internal sealed class Login : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("authentication/login", async (
            LoginRequest request,
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            return await Result.Create(request, GeneralErrors.UnprocessableRequest)
                .Map(request => new LoginUserCommand(request.Email, request.Password))
                .Bind(command => sender.Send(command, cancellationToken))
                .Match(Results.Ok, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Authentication)
        .RequireFeature(FeatureFlags.UseV1UsersApi)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter);
    }
}
