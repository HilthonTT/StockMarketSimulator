using FluentValidation.Results;
using Quartz;
using FluentValidation;
using Microsoft.Extensions.Logging;
using SharedKernel;
using Contracts.Emails;
using Application.Abstractions.Notifications;

namespace Modules.Users.BackgroundJobs.Users.PasswordChanged;

[DisallowConcurrentExecution]
public sealed class PasswordChangedJob(
    ILogger<PasswordChangedJob> logger,
    IValidator<PasswordChangedJobData> validator,
    INotificationService notificationService,
    IDateTimeProvider dateTimeProvider) : IJob
{
    public const string Name = nameof(PasswordChangedJob);

    public async Task Execute(IJobExecutionContext context)
    {
        JobDataMap data = context.MergedJobDataMap;
        CancellationToken ct = context.CancellationToken;

        var jobData = new PasswordChangedJobData
        {
            UserId = data.GetGuid("userId"),
            Email = data.GetString("email") ?? string.Empty,
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
            var request = new PasswordChangedEmail(jobData.Email, dateTimeProvider.UtcNow);

            await notificationService.SendPasswordChangedAsync(request, ct);

            logger.LogInformation("Successfully sent password changed to {Email} for user {UserId}.", jobData.Email, jobData.UserId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send assword changed to user {UserId}. Exception: {Message}", jobData.UserId, ex.Message);
            throw;
        }

        logger.LogInformation("{JobName} execution completed for user {UserId} at {Timestamp}", Name, jobData.UserId, dateTimeProvider.UtcNow);
    }
}
