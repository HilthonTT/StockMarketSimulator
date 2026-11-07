using Application.Abstractions.Notifications;
using Infrastructure.BackgroundJobs;
using Contracts.Emails;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Modules.Budgeting.Api.Api;
using Modules.Budgeting.Domain.ValueObjects;
using Modules.Users.BackgroundJobs.Users.EmailVerification;
using Quartz;
using SharedKernel;

namespace Modules.Users.BackgroundJobs.Users.UserEmailVerified;

public sealed class UserEmailVerifiedJob(
    ILogger<UserEmailVerifiedJob> logger,
    INotificationService notificationService,
    IDateTimeProvider dateTimeProvider,
    IBudgetingApi budgetingApi,
    IValidator<UserEmailVerifiedJobData> validator) : IJob
{
    public const string Name = nameof(EmailVerificationJob);

    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken = default)
    {
        JobDataMap data = context.MergedJobDataMap;
        CancellationToken ct = cancellationToken;

        var jobData = new UserEmailVerifiedJobData
        {
            UserId = data.GetGuid("userId"),
            Email = data.GetString("email") ?? string.Empty,
            Username = data.GetString("username") ?? string.Empty
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
            logger.LogDebug("Preparing to send welcome email to {Email} for user {UserId}.", jobData.Email, jobData.UserId);

            var request = new WelcomeEmail(jobData.Email, jobData.Username);

            Task sendWelcomeTask = notificationService.SendWelcomeAsync(request, cancellationToken);
            Task addBudgetTask = budgetingApi.AddBudgetAsync(jobData.UserId, Currency.Usd.Code, cancellationToken);

            await Task.WhenAll(sendWelcomeTask, addBudgetTask);

            logger.LogInformation("Successfully sent welcome email to {Email} for user {UserId}.", jobData.Email, jobData.UserId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to complete welcome process for user {UserId}. Exception: {Message}", jobData.UserId, ex.Message);
            throw;
        }
    }
}
