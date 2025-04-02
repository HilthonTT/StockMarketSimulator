using FluentValidation;
using Infrastructure;
using Infrastructure.Database.Interceptors;
using Infrastructure.Outbox;
using Infrastructure.Validation;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Modules.Stocks.Api;
using Modules.Stocks.Application.Abstractions.Data;
using Modules.Stocks.Application.Abstractions.Http;
using Modules.Stocks.Application.Abstractions.Realtime;
using Modules.Stocks.Domain.Repositories;
using Modules.Stocks.Infrastructure.Api;
using Modules.Stocks.Infrastructure.Database;
using Modules.Stocks.Infrastructure.Http;
using Modules.Stocks.Infrastructure.Realtime;
using Modules.Stocks.Infrastructure.Realtime.Options;
using Modules.Stocks.Infrastructure.Repositories;
using SharedKernel;

namespace Modules.Stocks.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddStocksInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly, includeInternalTypes: true);

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

        string? connectionString = configuration.GetConnectionString(ConfigurationNames.Database);
        Ensure.NotNullOrEmpty(connectionString, nameof(connectionString));

        services.AddDbContext<StocksDbContext>(
           (sp, options) => options
               .UseNpgsql(connectionString, npgsqlOptions =>
                   npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Stocks))
               .UseSnakeCaseNamingConvention()
               .AddInterceptors(
                   sp.GetRequiredService<InsertOutboxMessagesInterceptor>(),
                   sp.GetRequiredService<UpdateAuditableInterceptor>()));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<StocksDbContext>());

        services.AddScoped<IDbContext>(sp => sp.GetRequiredService<StocksDbContext>());

        services.AddScoped<IStockRepository, StockRepository>();
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
