using Application.Abstractions.Notifications;
using Contracts.Emails;
using Microsoft.Extensions.Logging;
using Quartz;
using SharedKernel;

namespace Modules.Users.BackgroundJobs.Users;

public sealed class EmailVerificationJob(
    ILogger<EmailVerificationJob> logger, 
    INotificationService notificationService,
    IDateTimeProvider dateTimeProvider) : IJob
{
    public const string Name = nameof(EmailVerificationJob);

    public async Task Execute(IJobExecutionContext context)
    {
        JobDataMap data = context.MergedJobDataMap;

        Guid userId = data.GetGuid("userId");
        string? email = data.GetString("email");
        string? verificationLink = data.GetString("verification-link");

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

        if (string.IsNullOrWhiteSpace(verificationLink))
        {
            logger.LogWarning("Execution aborted: Missing verification link for user {UserId}.", userId);
            return;
        }

        try
        {
            logger.LogDebug("Preparing to send verification email to {Email} for user {UserId}.", email, userId);

            var request = new EmailVerificationEmail(email, verificationLink);

            await notificationService.SendEmailVerificationAsync(request, context.CancellationToken);

            logger.LogInformation("Successfully sent verification email to {Email} for user {UserId}.", email, userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send verification email to user {UserId}. Exception: {Message}", userId, ex.Message);

            // Rethrow to let Quartz handle retry logic
            throw;
        }

        logger.LogInformation(
            "{JobName} execution completed for user {UserId} at {Timestamp}",
            Name, 
            userId, 
            dateTimeProvider.UtcNow);
    }
}
