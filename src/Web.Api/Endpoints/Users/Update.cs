using Application.Abstractions.Messaging;
using Modules.Users.Application.Users.Update;
using Modules.Users.Contracts.Users;
using Modules.Users.Domain.Enums;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Users;

internal sealed class Update : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPatch("users/{userId:guid}", async (
            Guid userId,
            UpdateUserRequest request,
            ICommandHandler<UpdateUserCommand> handler,
            CancellationToken cancellationToken) =>
        {
            return await Result.Create(request, GeneralErrors.UnprocessableRequest)
                .Map(request => new UpdateUserCommand(userId, request.Username))
                .Bind(command => handler.Handle(command, cancellationToken))
                .Match(Results.NoContent, CustomResults.Problem);
        })
        .WithOpenApi()
        .HasPermission(Permission.Read, Permission.Write)
        .WithTags(Tags.Users)
        .RequireFeature(FeatureFlags.UseV1UsersApi)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter);
    }
}
