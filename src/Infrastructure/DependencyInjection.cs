﻿using Application.Abstractions.Authentication;
using Application.Abstractions.Caching;
using Application.Abstractions.Data;
using Application.Abstractions.Emails;
using Application.Abstractions.Events;
using Application.Abstractions.Notifications;
using FluentValidation;
using Infrastructure.Authentication;
using Infrastructure.Caching;
using Infrastructure.Channels;
using Infrastructure.Database;
using Infrastructure.Emails;
using Infrastructure.Emails.Options;
using Infrastructure.Events;
using Infrastructure.Events.Options;
using Infrastructure.Notifications;
using Infrastructure.Time;
using Infrastructure.Validation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using SharedKernel;

namespace Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
       this IServiceCollection services,
       IConfiguration configuration)
    {
        services
            .AddServices()
            .AddEmail()
            .AddDatabase(configuration)
            .AddCaching(configuration)
            .AddHealthChecks(configuration)
            .AddMessaging();

        return services;
    }

    private static IServiceCollection AddServices(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.AddHttpContextAccessor();
        services.AddScoped<IUserContext, UserContext>();

        services.AddHostedService<ChannelNotificationsProcessor>();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString(ConfigurationNames.Database);
        Ensure.NotNullOrEmpty(connectionString);

        services.AddSingleton<IDbConnectionFactory>(_ =>
            new DbConnectionFactory(new NpgsqlDataSourceBuilder(connectionString).Build()));

        return services;
    }

    private static IServiceCollection AddCaching(this IServiceCollection services, IConfiguration configuration)
    {
        string? redisConnectionString = configuration.GetConnectionString(ConfigurationNames.Redis);
        Ensure.NotNullOrEmpty(redisConnectionString, nameof(redisConnectionString));

        services.AddStackExchangeRedisCache(options =>
            options.Configuration = redisConnectionString);

        services.AddSingleton<ICacheService, CacheService>();

        return services;
    }

    private static IServiceCollection AddEmail(this IServiceCollection services)
    {
        services.AddOptionsWithFluentValidation<EmailOptions>(EmailOptions.SettingsKey);

        services.AddTransient<IEmailService, EmailService>();
        services.AddTransient<INotificationService, NotificationService>();

        return services;
    }

    private static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddHealthChecks()
            .AddNpgSql(configuration.GetConnectionString(ConfigurationNames.Database)!)
            .AddRedis(configuration.GetConnectionString(ConfigurationNames.Redis)!);

        return services;
    }

    private static IServiceCollection AddMessaging(this IServiceCollection services)
    {
        services.AddSingleton<IEventBus, EventBus>();

        services.AddOptionsWithFluentValidation<MessageBrokerOptions>(MessageBrokerOptions.SettingsKey);

        return services;
    }
}
