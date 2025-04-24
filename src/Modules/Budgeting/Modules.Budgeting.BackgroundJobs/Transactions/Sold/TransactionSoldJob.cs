using Application.Abstractions.Notifications;
using Contracts.Emails;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.Budgeting.Application.Abstractions.Data;
using Modules.Budgeting.BackgroundJobs.Transactions.Bought;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Enums;
using Modules.Budgeting.Domain.Repositories;
using Quartz;
using SharedKernel;

namespace Modules.Budgeting.BackgroundJobs.Transactions.Sold;

[DisallowConcurrentExecution]
public sealed class TransactionSoldJob(
    ILogger<TransactionBoughtJob> logger,
    INotificationService notificationService,
    IDateTimeProvider dateTimeProvider,
    IServiceScopeFactory serviceScopeFactory,
    IValidator<SellConfirmedJobData> validator) : IJob
{
    public const string Name = nameof(TransactionSoldJob);

    public async Task Execute(IJobExecutionContext context)
    {
        CancellationToken ct = context.CancellationToken;
        JobDataMap data = context.MergedJobDataMap;

        var jobData = new SellConfirmedJobData
        {
            Ticker = data.GetString("ticker") ?? string.Empty,
            TransactionId = data.GetGuid("transaction-id"),
            TotalAmount = (decimal)data.GetDouble("total-amount"),
            Quantity = data.GetInt("quantity"),
            CreatedOnUtc = data.GetDateTime("created-on-utc"),
            UserEmail = data.GetString("user-email") ?? string.Empty,
            UserId = data.GetGuid("user-id")
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

        logger.LogInformation("Executing {JobName} at {Timestamp}", Name, dateTimeProvider.UtcNow);

        try
        {
            using IServiceScope scope = serviceScopeFactory.CreateScope(); 

            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
            var auditLogRepository = scope.ServiceProvider.GetRequiredService<IAuditLogRepository>();

            var request = new SellConfirmedEmail(
               jobData.UserEmail,
               jobData.Ticker,
               jobData.TotalAmount,
               dateTimeProvider.UtcNow);

            await notificationService.SendSellConfirmedAsync(request, ct);

            var auditLog = AuditLog.Create(
                userId: jobData.UserId,
                logType: AuditLogType.BuyStock,
                action: $"Sold {jobData.Quantity} shares",
                description: $"TransactionId={jobData.TransactionId}; Total={jobData.TotalAmount:C}; Time={jobData.CreatedOnUtc:O}",
                relatedEntityId: jobData.TransactionId,
                relatedEntityType: typeof(Transaction).Name
            );

            auditLogRepository.Insert(auditLog);
            await unitOfWork.SaveChangesAsync(ct);

            logger.LogInformation("Successfully sent sell confirmed email to {Email} for user {UserId}.", jobData.UserEmail, jobData.UserId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to execute sell confirmed job. Exception: {Message}", ex.Message);

            // Rethrow to let Quartz handle retry logic
            throw;
        }
    }
}
