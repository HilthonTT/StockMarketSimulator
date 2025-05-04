using EntityFramework.Exceptions.PostgreSQL;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Database;

internal sealed class GeneralDbContextFactory : IDesignTimeDbContextFactory<GeneralDbContext>
{
    public GeneralDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();

        string? connectionString = configuration.GetConnectionString(ConfigurationNames.Database);

        var optionsBuilder = new DbContextOptionsBuilder<GeneralDbContext>()
            .UseNpgsql(connectionString, npgsqlOptions =>
                npgsqlOptions.MigrationsHistoryTable(HistoryRepository.DefaultTableName, Schemas.General))
            .UseSnakeCaseNamingConvention()
            .UseExceptionProcessor();

        return new GeneralDbContext(optionsBuilder.Options);
    }
}
