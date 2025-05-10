using EntityFramework.Exceptions.PostgreSQL;
using Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;

namespace Modules.Stocks.Infrastructure.Database;

internal sealed class StocksDbContextFactory : IDesignTimeDbContextFactory<StocksDbContext>
{
    public StocksDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        string? connectionString = configuration.GetConnectionString(ConfigurationNames.Database);

        var optionsBuilder = new DbContextOptionsBuilder<StocksDbContext>()
            .UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.Stocks))
            .UseSnakeCaseNamingConvention()
            .UseExceptionProcessor();

        return new StocksDbContext(optionsBuilder.Options);
    }
}
