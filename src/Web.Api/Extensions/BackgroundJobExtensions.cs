using Modules.Stocks.Infrastructure.Outbox;
using Modules.Users.Infrastructure.Outbox;
using Quartz;

namespace Web.Api.Extensions;

public static class BackgroundJobExtensions
{
    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services)
    {
        services.AddQuartz(configure =>
        {
            var userJobKey = new JobKey(ProcessUserOutboxMessagesJob.Name);
            configure
                .AddJob<ProcessUserOutboxMessagesJob>(userJobKey)
                .AddTrigger(
                    trigger => trigger.ForJob(userJobKey).WithSimpleSchedule(
                        schedule => schedule.WithIntervalInSeconds(5).RepeatForever()));

            var stocksJobKey = new JobKey(ProcessStockOutboxMessagesJob.Name);
            configure
                .AddJob<ProcessStockOutboxMessagesJob>(stocksJobKey)
                .AddTrigger(
                    trigger => trigger.ForJob(stocksJobKey).WithSimpleSchedule(
                        schedule => schedule.WithIntervalInSeconds(5).RepeatForever()));
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        return services;
    }
}
