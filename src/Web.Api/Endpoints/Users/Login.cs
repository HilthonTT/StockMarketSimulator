using MediatR;
using Modules.Users.Application.Users.Login;
using Modules.Users.Contracts.Users;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class Login : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/login", async (
            LoginRequest request,
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            var command = new LoginUserCommand(request.Email, request.Password);

            Result<TokenResponse> result = await sender.Send(command, cancellationToken);

            return result.Match(Results.Ok, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Users);
    }
}
