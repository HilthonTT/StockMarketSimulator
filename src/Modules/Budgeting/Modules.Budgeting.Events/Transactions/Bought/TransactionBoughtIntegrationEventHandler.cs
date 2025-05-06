using Application.Abstractions.Messaging;
using Microsoft.Extensions.DependencyInjection;
using Modules.Budgeting.Application.Transactions.Buy;
using Modules.Budgeting.BackgroundJobs.Transactions.Bought;
using Modules.Budgeting.Domain.Entities;
using Modules.Budgeting.Domain.Errors;
using Modules.Budgeting.Domain.Repositories;
using Modules.Users.Api;
using Quartz;
using SharedKernel;

namespace Modules.Budgeting.Events.Transactions.Bought;

internal sealed class TransactionBoughtIntegrationEventHandler(
    IServiceScopeFactory serviceScopeFactory,
    ISchedulerFactory schedulerFactory) : IIntegrationEventHandler<TransactionBoughtIntegrationEvent>
{
    public async Task Handle(TransactionBoughtIntegrationEvent notification, CancellationToken cancellationToken)
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
            { "transaction-id", notification.TransactionId },
            { "total-amount", transaction.TotalAmount },
            { "quantity", transaction.Quantity },
            { "created-on-utc", transaction.CreatedOnUtc },

            { "user-email", user.Email },
            { "user-id", user.Id },
        };

        IJobDetail job = JobBuilder.Create<TransactionBoughtJob>()
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
