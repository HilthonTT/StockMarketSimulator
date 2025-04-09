using MediatR;
using Modules.Users.Application.Users.ResendEmailVerification;
using Modules.Users.Contracts.Users;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class ResendEmailVerification : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/resend-email-verification", async (
            ResendEmailVerificationRequest request,
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            var command = new ResendEmailVerificationCommand(request.Email);

            Result result = await sender.Send(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Users);
    }
}
