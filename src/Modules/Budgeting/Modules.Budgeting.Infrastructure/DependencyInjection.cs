using Infrastructure.Database.Interceptors;
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
using Modules.Budgeting.Api;
using Modules.Budgeting.Infrastructure.Api;

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

        string? connectionString = configuration.GetConnectionString(ConfigurationNames.Database);
        Ensure.NotNullOrEmpty(connectionString, nameof(connectionString));

        services.AddDbContext<BudgetingDbContext>(
           (sp, options) => options
               .UseNpgsql(connectionString, npgsqlOptions =>
                   npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Budgeting))
               .UseSnakeCaseNamingConvention()
               .AddInterceptors(
                   sp.GetRequiredService<InsertOutboxMessagesInterceptor>(),
                   sp.GetRequiredService<UpdateAuditableInterceptor>()));

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<BudgetingDbContext>());

        services.AddScoped<IDbContext>(sp => sp.GetRequiredService<BudgetingDbContext>());

        services.AddScoped<IBudgetRepository, BudgetRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();

        return services;
    }

    private static IServiceCollection AddApi(this IServiceCollection services)
    {
        services.AddScoped<IBudgetingApi, BudgetingApi>();

        return services;
    }
}
