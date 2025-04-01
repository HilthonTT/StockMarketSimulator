using Modules.Stocks.Infrastructure.Outbox;
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
            ConfigureRevokeExpiredTokensJob(configure);
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        return services;
    }

    private static void ConfigureUserOutboxJob(IServiceCollectionQuartzConfigurator configure)
    {
        var jobKey = new JobKey(ProcessUserOutboxMessagesJob.Name);
        configure
            .AddJob<ProcessUserOutboxMessagesJob>(jobKey)
            .AddTrigger(trigger => trigger.ForJob(jobKey).WithSimpleSchedule(
                schedule => schedule.WithIntervalInSeconds(5).RepeatForever()));
    }

    private static void ConfigureStockOutboxJob(IServiceCollectionQuartzConfigurator configure)
    {
        var jobKey = new JobKey(ProcessStockOutboxMessagesJob.Name);
        configure
            .AddJob<ProcessStockOutboxMessagesJob>(jobKey)
            .AddTrigger(trigger => trigger.ForJob(jobKey).WithSimpleSchedule(
                schedule => schedule.WithIntervalInSeconds(5).RepeatForever()));
    }

    private static void ConfigureRevokeExpiredTokensJob(IServiceCollectionQuartzConfigurator configure)
    {
        var jobKey = new JobKey(RevokeExpiredRefreshTokenJob.Name);
        configure
            .AddJob<RevokeExpiredRefreshTokenJob>(jobKey)
            .AddTrigger(trigger => trigger.ForJob(jobKey).WithSimpleSchedule(
                schedule => schedule.WithIntervalInSeconds(20).RepeatForever()));
    }
}
