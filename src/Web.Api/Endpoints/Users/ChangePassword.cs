using MediatR;
using Modules.Users.Application.Users.ChangePassword;
using Modules.Users.Contracts.Users;
using Modules.Users.Domain.Enums;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class ChangePassword : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("users/{userId:guid}/change-password", async (
            Guid userId,
            ChangePasswordRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new ChangePasswordCommand(userId, request.CurrentPassword, request.NewPassword);

            Result result = await sender.Send(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.Users)
        .HasPermission(Permission.Read, Permission.Write);
    }
}
