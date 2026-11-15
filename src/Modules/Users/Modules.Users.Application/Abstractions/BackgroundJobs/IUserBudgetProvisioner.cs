namespace Modules.Users.Application.Abstractions.BackgroundJobs;

/// <summary>
/// Gives a newly verified user their starting budget.
/// </summary>
/// <remarks>
/// Called from the email-verified integration-event handler, which is retried when it throws, so the
/// implementation must be idempotent: calling it again for a user who already has a budget is a no-op.
/// </remarks>
public interface IUserBudgetProvisioner
{
    Task EnsureBudgetAsync(Guid userId, CancellationToken cancellationToken = default);
}
