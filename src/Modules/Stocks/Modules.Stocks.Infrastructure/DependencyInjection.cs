﻿using EntityFramework.Exceptions.PostgreSQL;
using FluentValidation;
using Infrastructure;
using Infrastructure.Database.Interceptors;
using Infrastructure.Database.Options;
using Infrastructure.Outbox;
using Infrastructure.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using Modules.Stocks.Api.Api;
using Modules.Stocks.Application.Abstractions.Data;
using Modules.Stocks.Application.Abstractions.Http;
using Modules.Stocks.Application.Abstractions.Realtime;
using Modules.Stocks.Application.Abstractions.Url;
using Modules.Stocks.Domain.Repositories;
using Modules.Stocks.Infrastructure.Api;
using Modules.Stocks.Infrastructure.Database;
using Modules.Stocks.Infrastructure.Http;
using Modules.Stocks.Infrastructure.Realtime;
using Modules.Stocks.Infrastructure.Realtime.Options;
using Modules.Stocks.Infrastructure.Repositories;
using Modules.Stocks.Infrastructure.Url;
using SharedKernel;

namespace Modules.Stocks.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddStocksInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssembly(StocksInfrastructureAssembly.Instance, includeInternalTypes: true);

        services.AddScoped<IUrlShorteningService, UrlShorteningService>();

        services
            .AddDatabase(configuration)
            .AddRealtime()
            .AddHttpClients(configuration)
            .AddApis();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton<InsertOutboxMessagesInterceptor>();
        services.TryAddSingleton<UpdateAuditableInterceptor>();
        services.TryAddSingleton<SoftDeleteInterceptor>();

        string? connectionString = configuration.GetConnectionString(ConfigurationNames.Database);
        Ensure.NotNullOrEmpty(connectionString, nameof(connectionString));

        services.AddDbContext<StocksDbContext>(
           (sp, options) =>
           {
               var databaseOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;

               options.UseNpgsql(connectionString, npgsqlOptions =>
               {
                   npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Stocks);

                   npgsqlOptions.CommandTimeout(databaseOptions.CommandTimeout);
               });

               options.EnableDetailedErrors(databaseOptions.EnableDetailedErrors);
               options.EnableSensitiveDataLogging(databaseOptions.EnableSensitiveDataLogging);

               options.UseSnakeCaseNamingConvention();
               options.UseExceptionProcessor();

               options.AddInterceptors(
                    sp.GetRequiredService<InsertOutboxMessagesInterceptor>(),
                    sp.GetRequiredService<UpdateAuditableInterceptor>(),
                    sp.GetRequiredService<SoftDeleteInterceptor>());
           });

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<StocksDbContext>());

        services.AddScoped<IDbContext>(sp => sp.GetRequiredService<StocksDbContext>());

        services.AddScoped<IStockSearchResultRepository, StockSearchResultRepository>();

        return services;
    }

    private static IServiceCollection AddHttpClients(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddTransient<LoggingDelegatingHandler>();
        services.AddTransient<RetryDelegatingHandler>();

        services.AddHttpClient<IStocksClient, StocksClient>(httpClient =>
        {
            string? apiUrl = configuration["Stocks:ApiUrl"];
            Ensure.NotNullOrEmpty(apiUrl, nameof(apiUrl));

            httpClient.BaseAddress = new Uri(apiUrl);
        })
        .AddHttpMessageHandler<LoggingDelegatingHandler>()
        .AddHttpMessageHandler<RetryDelegatingHandler>();

        return services;
    }

    private static IServiceCollection AddRealtime(this IServiceCollection services)
    {
        services.AddSignalR();

        services.AddOptionsWithFluentValidation<StockUpdateOptions>(StockUpdateOptions.ConfigurationSection);

        services.AddSingleton<IStockVolatilityProvider, SimpleStockVolatilityProvider>();

        services.AddScoped<IStockService, StockService>();
        services.AddSingleton<IActiveTickerManager, ActiveTickerManager>();

        return services;
    }

    private static IServiceCollection AddApis(this IServiceCollection services)
    {
        services.AddScoped<IStocksApi, StocksApi>();

        return services;
    }
}
