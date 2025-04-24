using Application.Abstractions.Notifications;
using Contracts.Emails;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Modules.Budgeting.Application.Abstractions.Data;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Enums;
using Modules.Budgeting.Domain.Repositories;
using Quartz;
using SharedKernel;

namespace Modules.Budgeting.BackgroundJobs.Transactions;

[DisallowConcurrentExecution]
public sealed class TransactionBoughtJob(
    ILogger<TransactionBoughtJob> logger,
    INotificationService notificationService,
    IDateTimeProvider dateTimeProvider,
    IServiceScopeFactory serviceScopeFactory) : IJob
{
    public const string Name = nameof(TransactionBoughtJob);

    public async Task Execute(IJobExecutionContext context)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();

        CancellationToken ct = context.CancellationToken;
        JobDataMap data = context.MergedJobDataMap;

        IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        IAuditLogRepository auditLogRepository = scope.ServiceProvider.GetRequiredService<IAuditLogRepository>();

        Guid transactionId = data.GetGuid("transaction-id");
        decimal totalAmount = (decimal)data.GetDouble("total-amount");
        int quantity = data.GetInt("quantity");
        DateTime createdOnUtc = data.GetDateTime("created-on-utc");

        string? userEmail = data.GetString("user-email");
        Guid userId = data.GetGuid("user-id");

        logger.LogInformation("Executing {JobName} at {Timestamp}", Name, dateTimeProvider.UtcNow);

        if (transactionId == Guid.Empty)
        {
            logger.LogWarning("Execution aborted: Missing or invalid transactionId.");
            return;
        }

        if (userId == Guid.Empty)
        {
            logger.LogWarning("Execution aborted: Missing or invalid userId.");
            return;
        }

        if (string.IsNullOrWhiteSpace(userEmail))
        {
            logger.LogWarning("Execution aborted: Missing userEmail.");
            return;
        }

        if (quantity <= 0 || totalAmount <= 0)
        {
            logger.LogWarning("Execution aborted: Invalid quantity or totalAmount.");
            return;
        }

        try
        {
            var request = new PurchaseConfirmedEmail(userEmail, totalAmount, quantity, createdOnUtc);

            await notificationService.SendPurchaseConfirmedAsync(request, ct);

            var auditLog = AuditLog.Create(
                userId: userId,
                logType: AuditLogType.BuyStock,
                action: $"Bought {quantity} shares",
                description: $"TransactionId={transactionId}; Total={totalAmount:C}; Time={createdOnUtc:O}",
                relatedEntityId: transactionId,
                relatedEntityType: typeof(Transaction).Name
            );

            auditLogRepository.Insert(auditLog);

            await unitOfWork.SaveChangesAsync(ct);

            logger.LogInformation("Successfully sent purchase confirmed email to {Email} for user {UserId}.", userEmail, userId);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to send purchase confirmed email Exception: {Message}", ex.Message);

            // Rethrow to let Quartz handle retry logic
            throw;
        }
    }
}
