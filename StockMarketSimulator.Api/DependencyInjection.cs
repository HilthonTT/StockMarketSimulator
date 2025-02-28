using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Npgsql;
using Quartz;
using RedisRateLimiting.AspNetCore;
using SharedKernel;
using StackExchange.Redis;
using StockMarketSimulator.Api.Infrastructure;
using StockMarketSimulator.Api.Infrastructure.Authorization;
using StockMarketSimulator.Api.Infrastructure.Caching;
using StockMarketSimulator.Api.Infrastructure.Database;
using StockMarketSimulator.Api.Infrastructure.Email;
using StockMarketSimulator.Api.Modules.Stocks.Infrastructure;
using StockMarketSimulator.Api.Modules.Users.Infrastructure;
using StockMarketSimulator.Api.OpenApi;
using System.Diagnostics;

namespace StockMarketSimulator.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddPresentation(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddServices()
            .AddDatabase(configuration)
            .AddHealthChecks(configuration)
            .AddCaching(configuration)
            .AddEmailServices(configuration)
            .AddApiVersioningInternal()
            .AddAuthorizationInternal()
            .AddBackgroundJobs(configuration)
            .AddRateLimiting(configuration);

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddOpenApi();
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();
        services.AddCors();
        services.AddSignalR();

        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance =
                    $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";

                context.ProblemDetails.Extensions.TryAdd("requestId", context.HttpContext.TraceIdentifier);

                Activity? activity = context.HttpContext.Features.Get<IHttpActivityFeature>()?.Activity;
                context.ProblemDetails.Extensions.TryAdd("traceId", activity?.Id);
            };
        });

        return services;
    }

    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");
        Ensure.NotNullOrEmpty(connectionString, nameof(connectionString));

        string? redisConnectionString = configuration.GetConnectionString("Cache");
        Ensure.NotNullOrEmpty(redisConnectionString, nameof(redisConnectionString));

        services
            .AddHealthChecks()
            .AddRedis(redisConnectionString)
            .AddNpgSql(connectionString);

        return services;
    }

    private static IServiceCollection AddApiVersioningInternal(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1);
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'V";
            options.SubstituteApiVersionInUrl = true;
        });

        services.ConfigureOptions<ConfigureSwaggerGenOptions>();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");
        Ensure.NotNullOrEmpty(connectionString, nameof(connectionString));

        services.AddSingleton<IDbConnectionFactory>(_ =>
            new DbConnectionFactory(new NpgsqlDataSourceBuilder(connectionString).Build()));

        services.AddSingleton<DatabaseInitializationCompletionSignal>();

        return services;
    }

    private static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
    {
        string? redisConnectionString = configuration.GetConnectionString("Cache");
        Ensure.NotNullOrEmpty(redisConnectionString, nameof(redisConnectionString));

        services.AddStackExchangeRedisCache(options => options.Configuration = redisConnectionString);

        services.AddSingleton<ICacheService, CacheService>();

        return services;
    }

    private static IServiceCollection AddEmailServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddFluentEmail(configuration["Email:SenderEmail"], configuration["Email:Sender"])
            .AddSmtpSender(configuration["Email:Host"], configuration.GetValue<int>("Email:Port"));

        services.AddScoped<IEmailService, EmailService>();

        return services;
    }

    private static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
    {
        services.AddAuthorization();

        services.AddScoped<IPermissionProvider, PermissionProvider>();

        services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        return services;
    }

    private static IServiceCollection AddBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("Database");
        Ensure.NotNullOrEmpty(connectionString, nameof(connectionString));

        services.Configure<StockUpdateOptions>(configuration.GetSection("StockUpdateOptions"));

        services.AddQuartz(options =>
        {
            options.AddDatabaseInitializerJob();
            options.AddRevokeExpiredRefreshTokenJob();
            options.AddStocksFeedUpdaterJob(services);
        });

        services.AddQuartzHostedService(options =>
        {
            options.WaitForJobsToComplete = true;
        });

        return services;
    }

    private static IServiceCollectionQuartzConfigurator AddRevokeExpiredRefreshTokenJob(
        this IServiceCollectionQuartzConfigurator options)
    {
        var jobKey = JobKey.Create(RevokeExpiredRefreshTokenBackgroundJob.Name);

        options.AddJob<RevokeExpiredRefreshTokenBackgroundJob>(jobKey)
               .AddTrigger(trigger =>
                    trigger.ForJob(jobKey)
                           .WithSimpleSchedule(schedule =>
                                schedule.WithIntervalInSeconds(5).RepeatForever()));

        return options;
    }

    private static IServiceCollectionQuartzConfigurator AddStocksFeedUpdaterJob(
        this IServiceCollectionQuartzConfigurator options, 
        IServiceCollection services)
    {
        using var scope = services.BuildServiceProvider().CreateScope();
        var stockUpdateOptions = scope.ServiceProvider.GetRequiredService<IOptions<StockUpdateOptions>>().Value;

        var jobKey = JobKey.Create(StocksFeedUpdater.Name);

        options.AddJob<StocksFeedUpdater>(jobKey)
          .AddTrigger(trigger =>
                    trigger.ForJob(jobKey)
                           .WithSimpleSchedule(schedule =>
                                schedule.WithIntervalInSeconds(1).RepeatForever()));

        return options;
    }

    private static IServiceCollectionQuartzConfigurator AddDatabaseInitializerJob(
        this IServiceCollectionQuartzConfigurator options)
    {
        var jobKey = JobKey.Create(DatabaseInitializer.Name);

        options.AddJob<DatabaseInitializer>(jobKey)
               .AddTrigger(trigger => trigger
                    .ForJob(jobKey)
                    .StartNow() // Execute immediately
                    .WithSimpleSchedule(schedule =>
                        schedule.WithRepeatCount(0) // No repeats
                                .WithInterval(TimeSpan.FromMilliseconds(1))));

        return options;
    }

    private static IServiceCollection AddRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        string? redisConnectionString = configuration.GetConnectionString("Cache");
        Ensure.NotNullOrEmpty(redisConnectionString, nameof(redisConnectionString));

        var connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.AddRedisSlidingWindowLimiter("sliding_window", (opt) =>
            {
                opt.ConnectionMultiplexerFactory = () => connectionMultiplexer;
                opt.PermitLimit = 15;
                opt.Window = TimeSpan.FromSeconds(10);
            });
        });

        return services;
    }
}
