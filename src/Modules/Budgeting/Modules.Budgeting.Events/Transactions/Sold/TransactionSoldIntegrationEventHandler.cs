using Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Modules.Budgeting.Application.Transactions.Sell;
using Modules.Budgeting.BackgroundJobs.Transactions.Sold;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Errors;
using Modules.Budgeting.Domain.Repositories;
using Modules.Users.Api.Api;
using Modules.Users.Api.Responses;
using Quartz;
using SharedKernel;

namespace Modules.Budgeting.Events.Transactions.Sold;

internal sealed class TransactionSoldIntegrationEventHandler(
    IServiceScopeFactory serviceScopeFactory,
    ISchedulerFactory schedulerFactory) : IIntegrationEventHandler<TransactionSoldIntegrationEvent>
{
    public async Task Handle(TransactionSoldIntegrationEvent notification, CancellationToken cancellationToken)
    {
        using IServiceScope scope = serviceScopeFactory.CreateScope();

        var transactionRepository = scope.ServiceProvider.GetRequiredService<ITransactionRepository>();
        var usersApi = scope.ServiceProvider.GetRequiredService<IUsersApi>();

        Option<Transaction> optionTransaction =
            await transactionRepository.GetByIdAsync(notification.TransactionId, cancellationToken);

        if (!optionTransaction.IsSome)
        {
            throw new DomainException(TransactionErrors.NotFound(notification.TransactionId));
        }

        Transaction transaction = optionTransaction.ValueOrThrow();

        Option<UserApiResponse> optionUser = await usersApi.GetByIdAsync(transaction.UserId, cancellationToken);
        if (!optionUser.IsSome)
        {
            throw new DomainException(UserErrors.NotFound(transaction.UserId));
        }

        UserApiResponse user = optionUser.ValueOrThrow();

        IScheduler scheduler = await schedulerFactory.GetScheduler(cancellationToken);

        var jobData = new JobDataMap
        {
            { "ticker", transaction.Ticker },
            { "transaction-id", notification.TransactionId },
            { "total-amount", transaction.TotalAmount },
            { "quantity", transaction.Quantity },
            { "created-on-utc", transaction.CreatedOnUtc },

            { "user-email", user.Email },
            { "user-id", user.Id },
        };

        IJobDetail job = JobBuilder.Create<TransactionSoldJob>()
           .WithIdentity($"notifier-{Guid.CreateVersion7()}", "email-notifier")
           .SetJobData(jobData)
           .Build();

        ITrigger trigger = TriggerBuilder.Create()
            .WithIdentity($"trigger-{Guid.CreateVersion7()}", "email-notifier")
            .StartNow()
            .Build();

        await scheduler.ScheduleJob(job, trigger, cancellationToken);
    }
}
