using Dapper;
using Npgsql;
using SharedKernel;

namespace StockMarketSimulator.Api.Infrastructure.Database;

internal sealed class DatabaseInitializer : BackgroundService
{
    private readonly IDbConnectionFactory _dbConnectionFactory;
    private readonly ILogger<DatabaseInitializer> _logger;
    private readonly IConfiguration _configuration;
    private readonly DatabaseInitializationCompletionSignal _signal;

    public DatabaseInitializer(
        IDbConnectionFactory dbConnectionFactory,
        ILogger<DatabaseInitializer> logger,
        IConfiguration configuration,
        DatabaseInitializationCompletionSignal signal)
    {
        _dbConnectionFactory = dbConnectionFactory;
        _logger = logger;
        _configuration = configuration;
        _signal = signal;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            _logger.LogInformation("Starting database initialization.");

            await EnsureDatabaseExistsAsync(stoppingToken);

            await InitializeDatabaseAsync(stoppingToken);

            _logger.LogInformation("Database initialization completed successfully.");

            _signal.MarkInitializationComplete();
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "An error occured while initializing the database.");
        }
    }

    private async Task EnsureDatabaseExistsAsync(CancellationToken cancellationToken)
    {
        string? dbConnectionString = _configuration.GetConnectionString("Database");
        Ensure.NotNullOrEmpty(dbConnectionString, nameof(dbConnectionString));

        var builder = new NpgsqlConnectionStringBuilder(dbConnectionString);
        string? databaseName = builder.Database;

        builder.Database = "postgres"; // Connect to the default 'postgres' database

        await using var connection = new NpgsqlConnection(builder.ConnectionString);
        try
        {
            await connection.OpenAsync(cancellationToken);

            bool databaseExists = await DatabaseExistsAsync(connection, databaseName);
            if (!databaseExists)
            {
                _logger.LogInformation("Creating database {DatabaseName}", databaseName);

                await CreateDatabaseAsync(connection, databaseName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error ensuring database {DatabaseName} exists.", databaseName);
            throw;
        }
    }

    private static async Task<bool> DatabaseExistsAsync(NpgsqlConnection connection, string? databaseName)
    {
        Ensure.NotNullOrEmpty(databaseName, nameof(databaseName));

        const string sql =
            """
            SELECT EXISTS(SELECT 1 FROM pg_database WHERE datname = @databaseName)
            """;

        bool databaseExists = await connection.ExecuteScalarAsync<bool>(sql, new { databaseName });

        return databaseExists;
    }

    private static async Task CreateDatabaseAsync(NpgsqlConnection connection, string? databaseName)
    {
        Ensure.NotNullOrEmpty(databaseName, nameof(databaseName));

        string sanitizedDatabaseName = databaseName.Replace("\"", "\"\"");

        string sql = $@"
            CREATE DATABASE ""{sanitizedDatabaseName}""
        ";

        await connection.ExecuteAsync(sql);
    }

    private async Task InitializeDatabaseAsync(CancellationToken cancellationToken)
    {
        await using var connection = await _dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        const string sql =
            """
            -- Check if users table exists, if not, create it.
            CREATE TABLE IF NOT EXISTS public.users (
                id UUID PRIMARY KEY,
                email VARCHAR(512) NOT NULL UNIQUE,
                user_name VARCHAR(256) NOT NULL,
                password_hash TEXT NOT NULL
            );
            """;

        await connection.ExecuteAsync(sql);
    }
}
