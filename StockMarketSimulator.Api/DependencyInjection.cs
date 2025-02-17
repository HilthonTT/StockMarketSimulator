using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Features;
using Npgsql;
using SharedKernel;
using StockMarketSimulator.Api.Infrastructure;
using StockMarketSimulator.Api.Infrastructure.Authorization;
using StockMarketSimulator.Api.Infrastructure.Caching;
using StockMarketSimulator.Api.Infrastructure.Database;
using StockMarketSimulator.Api.Infrastructure.Email;
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
            .AddAuthorizationInternal();

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

        services.AddHostedService<DatabaseInitializer>();

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
}
