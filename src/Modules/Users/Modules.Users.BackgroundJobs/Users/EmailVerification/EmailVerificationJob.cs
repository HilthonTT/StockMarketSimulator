using Application.Abstractions.Notifications;
using Contracts.Emails;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Quartz;
using SharedKernel;

namespace Modules.Users.BackgroundJobs.Users.EmailVerification;

public sealed class EmailVerificationJob(
    ILogger<EmailVerificationJob> logger, 
    INotificationService notificationService,
    IDateTimeProvider dateTimeProvider,
    IValidator<EmailVerificationJobData> validator) : IJob
{
    public const string Name = nameof(EmailVerificationJob);

    public async Task Execute(IJobExecutionContext context)
    {
        JobDataMap data = context.MergedJobDataMap;
        CancellationToken ct = context.CancellationToken;

        var jobData = new EmailVerificationJobData
        {
            UserId = data.GetGuid("userId"),
            Email = data.GetString("email") ?? string.Empty,
            VerificationLink = data.GetString("verification-link") ?? string.Empty
        };

        ValidationResult validationResult = await validator.ValidateAsync(jobData, ct);
        if (!validationResult.IsValid)
        {
            foreach (ValidationFailure error in validationResult.Errors)
            {
                logger.LogWarning("Validation failed: {Error}", error.ErrorMessage);
            }
            return;
        }

        logger.LogInformation("Executing {JobName} for user {UserId} at {Timestamp}", Name, jobData.UserId, dateTimeProvider.UtcNow);

        try
        {
            logger.LogDebug("Preparing to send verification email to {Email} for user {UserId}.", jobData.Email, jobData.UserId);

            var request = new EmailVerificationEmail(jobData.Email, jobData.VerificationLink);

            await notificationService.SendEmailVerificationAsync(request, ct);

            logger.LogInformation("Successfully sent verification email to {Email} for user {UserId}.", jobData.Email, jobData.UserId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send verification email to user {UserId}. Exception: {Message}", jobData.UserId, ex.Message);
            throw;
        }

        logger.LogInformation("{JobName} execution completed for user {UserId} at {Timestamp}", Name, jobData.UserId, dateTimeProvider.UtcNow);
    }
}
