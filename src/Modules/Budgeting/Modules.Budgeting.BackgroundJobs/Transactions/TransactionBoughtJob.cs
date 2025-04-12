using Application.Abstractions.Notifications;
using Contracts.Emails;
using Microsoft.Extensions.Logging;
using Quartz;
using SharedKernel;

namespace Modules.Budgeting.BackgroundJobs.Transactions;

[DisallowConcurrentExecution]
public sealed class TransactionBoughtJob(
    ILogger<TransactionBoughtJob> logger,
    INotificationService notificationService,
    IDateTimeProvider dateTimeProvider) : IJob
{
    public const string Name = nameof(TransactionBoughtJob);

    public async Task Execute(IJobExecutionContext context)
    {
        CancellationToken ct = context.CancellationToken;
        JobDataMap data = context.MergedJobDataMap;

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
