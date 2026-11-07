using Infrastructure.Events;
using Microsoft.Extensions.Options;
using Modules.Budgeting.Infrastructure.Outbox;
using Modules.Stocks.BackgroundJobs.Stocks;
using Modules.Stocks.Infrastructure.Outbox;
using Modules.Stocks.Infrastructure.Realtime;
using Modules.Stocks.Infrastructure.Realtime.Options;
using Modules.Users.BackgroundJobs.Users.RevokeExpiredRefreshTokens;
using Modules.Users.Infrastructure.Outbox;
using Quartz;

namespace Web.Api.Extensions;

public static class BackgroundJobExtensions
{
    private static readonly TimeSpan OutboxInterval = TimeSpan.FromSeconds(5);

    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services)
    {
        services.AddQuartz(quartz =>
        {
            // Outbox processors (one per module schema)
            quartz.AddRecurringJob<ProcessUserOutboxMessagesJob>(OutboxInterval);
            quartz.AddRecurringJob<ProcessStockOutboxMessagesJob>(OutboxInterval);
            quartz.AddRecurringJob<ProcessBudgetingOutboxMessagesJob>(OutboxInterval);

            // Cross-module integration events (RabbitMQ consumer)
            quartz.AddRecurringJob<IntegrationEventProcessorJob>(TimeSpan.FromSeconds(5));

            // Maintenance
            quartz.AddRecurringJob<RevokeExpiredRefreshTokenJob>(TimeSpan.FromSeconds(20));
            quartz.AddRecurringJob<UpdateLatestStockPriceJob>(TimeSpan.FromMinutes(1));

            // Real-time price feed: the interval comes from validated options, resolved from the
            // real container instead of building a throw-away second service provider.
            quartz.ScheduleJob<StocksFeedUpdater>(
                (serviceProvider, trigger) =>
                {
                    StockUpdateOptions options =
                        serviceProvider.GetRequiredService<IOptions<StockUpdateOptions>>().Value;

                    trigger
                        .WithIdentity($"{StocksFeedUpdater.Name}-trigger")
                        .WithSimpleSchedule(schedule => schedule
                            .WithInterval(TimeSpan.FromSeconds(options.UpdateIntervalInSeconds))
                            .RepeatForever());
                },
                (_, job) => job.WithIdentity(StocksFeedUpdater.Name));
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        return services;
    }

    private static void AddRecurringJob<TJob>(this IQuartzBuilder quartz, TimeSpan interval)
        where TJob : class, IJob
    {
        string name = typeof(TJob).Name;

        quartz.ScheduleJob<TJob>(
            trigger => trigger
                .WithIdentity($"{name}-trigger")
                .WithSimpleSchedule(schedule => schedule.WithInterval(interval).RepeatForever()),
            job => job.WithIdentity(name));
    }
}
