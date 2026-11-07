using Infrastructure.Outbox;
using Modules.Users.Domain.Enums;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Outbox;

internal sealed class GetFailures : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("outbox/failures", async (
            OutboxAdministration outboxAdministration,
            CancellationToken cancellationToken = default) =>
        {
            IReadOnlyList<OutboxFailureSummary> failures = await outboxAdministration.GetFailuresAsync(cancellationToken);

            return Results.Ok(failures);
        })
        .WithTags(Tags.Outbox)
        .HasPermission(Permission.ManageOutbox);
    }
}
