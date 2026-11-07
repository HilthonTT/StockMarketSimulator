using Infrastructure.Outbox;
using Modules.Users.Domain.Enums;
using Web.Api.Extensions;

namespace Web.Api.Endpoints.Outbox;

internal sealed class Redrive : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // Resets the failed messages of one module's outbox so the next outbox run dispatches them again.
        app.MapPost("outbox/{schema}/redrive", async (
            string schema,
            OutboxAdministration outboxAdministration,
            CancellationToken cancellationToken = default) =>
        {
            if (!outboxAdministration.IsKnownSchema(schema))
            {
                return Results.NotFound($"Unknown outbox schema '{schema}'.");
            }

            int redriven = await outboxAdministration.RedriveFailedAsync(schema, cancellationToken);

            return Results.Ok(new { schema, redriven });
        })
        .WithTags(Tags.Outbox)
        .HasPermission(Permission.ManageOutbox);
    }
}
