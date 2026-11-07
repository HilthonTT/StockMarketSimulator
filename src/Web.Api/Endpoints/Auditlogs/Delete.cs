using Application.Abstractions.Messaging;
using Modules.Budgeting.Application.Auditlogs.Delete;
using Modules.Users.Domain.Enums;
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
                .Bind(command => sender.Send(command, cancellationToken))
                .Match(Results.NoContent, CustomResults.Problem);
        })
        .WithTags(Tags.AuditLogs)
        .HasPermission(Permission.Read, Permission.Write)
        .RequireFeature(FeatureFlags.UseV1BudgetingApi)
        .RequireRateLimiting(RateLimiterPolicyNames.GlobalLimiter);
    }
}
