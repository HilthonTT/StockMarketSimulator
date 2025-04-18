using Infrastructure.Events;
using Microsoft.Extensions.Options;
using Modules.Budgeting.Infrastructure.Outbox;
using Modules.Stocks.Infrastructure.Outbox;
using Modules.Stocks.Infrastructure.Realtime;
using Modules.Stocks.Infrastructure.Realtime.Options;
using Modules.Users.BackgroundJobs.Users;
using Modules.Users.Infrastructure.Outbox;
using Quartz;

namespace Web.Api.Extensions;

public static class BackgroundJobExtensions
{
    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services)
    {
        services.AddQuartz(configure =>
        {
            ConfigureUserOutboxJob(configure);
            ConfigureStockOutboxJob(configure);
            ConfigureBudgetingOutboxJob(configure);
            ConfigureRevokeExpiredTokensJob(configure);
            ConfigureStocksFeedUpdaterJob(configure, services);
            ConfigureIntegrationEventProcessorJob(configure);
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        return services;
    }

    private static void ConfigureUserOutboxJob(IServiceCollectionQuartzConfigurator configure)
    {
        JobKey jobKey = JobKey.Create(ProcessUserOutboxMessagesJob.Name);
        configure
            .AddJob<ProcessUserOutboxMessagesJob>(jobKey)
            .AddTrigger(trigger => trigger.ForJob(jobKey).WithSimpleSchedule(
                schedule => schedule.WithIntervalInSeconds(5).RepeatForever()));
    }

    private static void ConfigureStockOutboxJob(IServiceCollectionQuartzConfigurator configure)
    {
        JobKey jobKey = JobKey.Create(ProcessStockOutboxMessagesJob.Name);
        configure
            .AddJob<ProcessStockOutboxMessagesJob>(jobKey)
            .AddTrigger(trigger => trigger.ForJob(jobKey).WithSimpleSchedule(
                schedule => schedule.WithIntervalInSeconds(5).RepeatForever()));
    }

    private static void ConfigureBudgetingOutboxJob(IServiceCollectionQuartzConfigurator configure)
    {
        JobKey jobKey = JobKey.Create(ProcessBudgetingOutboxMessagesJob.Name);
        configure
            .AddJob<ProcessBudgetingOutboxMessagesJob>(jobKey)
            .AddTrigger(trigger => trigger.ForJob(jobKey).WithSimpleSchedule(
                schedule => schedule.WithIntervalInSeconds(5).RepeatForever()));
    }

    private static void ConfigureRevokeExpiredTokensJob(IServiceCollectionQuartzConfigurator configure)
    {
        JobKey jobKey = JobKey.Create(RevokeExpiredRefreshTokenJob.Name);
        configure
            .AddJob<RevokeExpiredRefreshTokenJob>(jobKey)
            .AddTrigger(trigger => trigger.ForJob(jobKey).WithSimpleSchedule(
                schedule => schedule.WithIntervalInSeconds(20).RepeatForever()));
    }

    private static void ConfigureStocksFeedUpdaterJob(
        IServiceCollectionQuartzConfigurator configure, 
        IServiceCollection services)
    {
        using IServiceScope scope = services.BuildServiceProvider().CreateScope();
        var stockUpdateOptions = scope.ServiceProvider.GetRequiredService<IOptions<StockUpdateOptions>>().Value;

        JobKey jobKey = JobKey.Create(StocksFeedUpdater.Name);

        configure.AddJob<StocksFeedUpdater>(jobKey)
          .AddTrigger(trigger =>
                trigger.ForJob(jobKey)
                    .WithSimpleSchedule(schedule =>
                        schedule.WithIntervalInSeconds(stockUpdateOptions.UpdateIntervalInSeconds).RepeatForever()));
    }

    private static void ConfigureIntegrationEventProcessorJob(IServiceCollectionQuartzConfigurator configure)
    {
        JobKey jobKey = JobKey.Create(IntegrationEventProcessorJob.Name);

        configure
            .AddJob<IntegrationEventProcessorJob>(jobKey)
            .AddTrigger(trigger => trigger.ForJob(jobKey).WithSimpleSchedule(
                schedule => schedule.WithIntervalInSeconds(5).RepeatForever()));
    }
}
