﻿using Infrastructure.Database.Interceptors;
using Infrastructure.Outbox;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SharedKernel;
using Modules.Budgeting.Infrastructure.Database;
using Microsoft.EntityFrameworkCore.Migrations;
using Modules.Budgeting.Application.Abstractions.Data;
using Modules.Budgeting.Domain.Repositories;
using Modules.Budgeting.Infrastructure.Repositories;
using Modules.Budgeting.Infrastructure.Api;
using EntityFramework.Exceptions.PostgreSQL;
using Modules.Budgeting.Api.Api;
using Infrastructure.Database.Options;
using Microsoft.Extensions.Options;

namespace Modules.Budgeting.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddBudgetingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services
            .AddDatabase(configuration)
            .AddApi();

        return services;
    }

    private static IServiceCollection AddDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        services.TryAddSingleton<InsertOutboxMessagesInterceptor>();
        services.TryAddSingleton<UpdateAuditableInterceptor>();
        services.TryAddSingleton<SoftDeleteInterceptor>();

        string? connectionString = configuration.GetConnectionString(ConfigurationNames.Database);
        Ensure.NotNullOrEmpty(connectionString, nameof(connectionString));

        services.AddDbContext<BudgetingDbContext>(
           (sp, options) =>
           {
               var databaseOptions = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;

               options.UseNpgsql(connectionString, npgsqlOptions =>
               {
                   npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Budgeting);

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

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<BudgetingDbContext>());

        services.AddScoped<IDbContext>(sp => sp.GetRequiredService<BudgetingDbContext>());

        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<IAuditLogRepository, AuditLogRepository>();

        return services;
    }

    private static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddScoped<IBudgetingApi, BudgetingApi>();

        return services;
    }
}
