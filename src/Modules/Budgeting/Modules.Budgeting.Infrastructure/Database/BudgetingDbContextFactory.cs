using EntityFramework.Exceptions.PostgreSQL;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;

namespace Modules.Budgeting.Infrastructure.Database;

internal sealed class BudgetingDbContextFactory : IDesignTimeDbContextFactory<BudgetingDbContext>
{
    public BudgetingDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        string? connectionString = configuration.GetConnectionString(ConfigurationNames.Database);

        var optionsBuilder = new DbContextOptionsBuilder<BudgetingDbContext>()
            .UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Budgeting))
            .UseSnakeCaseNamingConvention()
            .UseExceptionProcessor();

        return new BudgetingDbContext(optionsBuilder.Options);
    }
}
