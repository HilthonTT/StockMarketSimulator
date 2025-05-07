using MediatR;
using Modules.Budgeting.Application.Auditlogs.Delete;
using SharedKernel;
using Web.Api.Extensions;
using Web.Api.Features;
using Web.Api.Infrastructure;

namespace Web.Api.Endpoints.Auditlogs;

internal sealed class Delete : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapDelete("auditlogs/{auditlogId:guid}", async (
            Guid auditlogId,
            ISender sender,
            CancellationToken cancellationToken = default) =>
        {
            return await Result.Success(new DeleteAuditlogCommand(auditlogId))
                .Bind(query => sender.Send(query, cancellationToken))
                .Match(Results.NoContent, CustomResults.Problem);
        })
        .WithOpenApi()
        .WithTags(Tags.AuditLogs)
        .RequireFeature(FeatureFlags.UseV1UsersApi)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter);
    }
}
