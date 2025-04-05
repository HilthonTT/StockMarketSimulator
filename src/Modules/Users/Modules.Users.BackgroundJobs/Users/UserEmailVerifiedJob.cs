using Application.Abstractions.Notifications;
using Contracts.Emails;
using Microsoft.Extensions.Logging;
using Modules.Budgeting.Api;
using Quartz;
using SharedKernel;

namespace Modules.Users.BackgroundJobs.Users;

public sealed class UserEmailVerifiedJob(
    ILogger<UserEmailVerifiedJob> logger,
    INotificationService notificationService,
    IDateTimeProvider dateTimeProvider,
    IBudgetingApi budgetingApi) : IJob
{
    public const string Name = nameof(EmailVerificationJob);

    public async Task Execute(IJobExecutionContext context)
    {
        JobDataMap data = context.MergedJobDataMap;

        Guid userId = data.GetGuid("userId");
        string? email = data.GetString("email");
        string? username = data.GetString("username");

        logger.LogInformation("Executing {JobName} for user {UserId} at {Timestamp}", Name, userId, dateTimeProvider.UtcNow);

        if (userId == Guid.Empty)
        {
            logger.LogWarning("Execution aborted: Missing or invalid userId.");
            return;
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            logger.LogWarning("Execution aborted: Missing or invalid email for user {UserId}.", userId);
            return;
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            logger.LogWarning("Execution aborted: Missing or invalid username for user {UserId}.", userId);
            return;
        }

        try
        {
            logger.LogDebug("Preparing to send welcome email to {Email} for user {UserId}.", email, userId);

            var request = new WelcomeEmail(email, username);

            var sendWelcomeTask = notificationService.SendWelcomeAsync(request, context.CancellationToken);
            var addBudgetTask = budgetingApi.AddBudgetAsync(userId, context.CancellationToken);

            await Task.WhenAll(sendWelcomeTask, addBudgetTask);

            logger.LogInformation("Successfully sent welcome email to {Email} for user {UserId}.", email, userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send welcome email to user {UserId}. Exception: {Message}", userId, ex.Message);

            // Rethrow to let Quartz handle retry logic
            throw;
        }
    }
}
