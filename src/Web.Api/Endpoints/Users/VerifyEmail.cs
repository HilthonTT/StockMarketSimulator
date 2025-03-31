using MediatR;
using Modules.Users.Application.Users;
using Modules.Users.Application.Users.VerifyEmail;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class VerifyEmail : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/verify-email", async (
            Guid token,
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            var command = new VerifyEmailCommand(token);

            Result result = await sender.Send(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Users)
        .WithName(UserEndpoints.VerifyEmail);
    }
}
