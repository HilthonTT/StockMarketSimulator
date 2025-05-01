using MediatR;
using Modules.Users.Application.Users.ResendEmailVerification;
using Modules.Users.Contracts.Users;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
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
            return await Result.Create(request, GeneralErrors.UnprocessableRequest)
                .Map(request => new ResendEmailVerificationCommand(request.Email))
                .Bind(command => sender.Send(command, cancellationToken))
                .Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.Users)
        .RequireFeature(FeatureFlags.UseV1UsersApi);
    }
}
