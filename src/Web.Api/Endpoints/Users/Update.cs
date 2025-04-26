using MediatR;
using Modules.Users.Application.Users.Update;
using Modules.Users.Contracts.Users;
using Modules.Users.Domain.Enums;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class Update : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("users/{userId:guid}", async (
            Guid userId,
            UpdateUserRequest request,
            ISender sender,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateUserCommand(userId, request.Username);

            Result result = await sender.Send(command, cancellationToken);

            return result.Match(Results.NoContent, CustomResults.Problem);
        })
        .WithOpenApi()
        .HasPermission(Permission.Read, Permission.Write)
        .WithTags(Tags.Users);
    }
}
