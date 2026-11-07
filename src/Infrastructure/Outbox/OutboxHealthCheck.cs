using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Outbox;

/// <summary>
/// Reports <see cref="HealthStatus.Degraded"/> while any outbox has messages with <c>error IS NOT NULL</c>.
/// </summary>
/// <remarks>
/// The check is also run every 30 seconds by <see cref="OutboxHealthLogPublisher"/>, which logs a warning
/// while failures exist, so a log-based alert works without anyone polling <c>/health</c>.
/// </remarks>
public sealed class OutboxHealthCheck(OutboxAdministration outboxAdministration) : IHealthCheck
{
    public const string Name = "outbox";
    public const string Tag = "outbox";

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<OutboxFailureSummary> failures = await outboxAdministration.GetFailuresAsync(cancellationToken);

        var data = failures.ToDictionary(f => f.Schema, f => (object)f.FailedCount);

        int total = failures.Sum(f => f.FailedCount);
        if (total == 0)
        {
            return HealthCheckResult.Healthy("No failed outbox messages.", data);
        }

        return new HealthCheckResult(
            context.Registration.FailureStatus,
            $"{total} outbox message(s) failed. Fix the cause, then re-drive them (POST api/v1/outbox/{{schema}}/redrive).",
            data: data);
    }
}

/// <summary>
/// Logs the outbox health check's result whenever it isn't healthy (log-based alerting).
/// </summary>
public sealed class OutboxHealthLogPublisher(ILogger<OutboxHealthLogPublisher> logger) : IHealthCheckPublisher
{
    public Task PublishAsync(HealthReport report, CancellationToken cancellationToken)
    {
        if (report.Entries.TryGetValue(OutboxHealthCheck.Name, out HealthReportEntry entry) &&
            entry.Status != HealthStatus.Healthy)
        {
            logger.LogWarning(
                entry.Exception,
                "Outbox health is {Status}: {Description} Failed messages per schema: {@FailedBySchema}",
                entry.Status,
                entry.Description,
                entry.Data);
        }

        return Task.CompletedTask;
    }
}
