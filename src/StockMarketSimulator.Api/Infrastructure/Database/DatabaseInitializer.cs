using Dapper;
using Npgsql;
using Quartz;
using SharedKernel;

namespace StockMarketSimulator.Api.Infrastructure.Database;

[DisallowConcurrentExecution]
internal sealed class DatabaseInitializer : IJob
{
    public const string Name = nameof(DatabaseInitializer);

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

    public async Task Execute(IJobExecutionContext context)
    {
        try
        {
            _logger.LogInformation("Starting database initialization.");

            await EnsureDatabaseExistsAsync(context.CancellationToken);

            await InitializeDatabaseAsync(context.CancellationToken);

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

            -- Check if refresh tokens table exists, if not, create it.
            CREATE TABLE IF NOT EXISTS public.refresh_tokens (
                id UUID PRIMARY KEY,
                token VARCHAR(200) NOT NULL UNIQUE,
                user_id UUID NOT NULL,
                expires_on_utc TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'UTC'),

                CONSTRAINT fk_refresh_tokens_users FOREIGN KEY (user_id) 
                REFERENCES public.users (id) ON DELETE CASCADE
            );

            -- Check if stock prices table exists, if not, create it.
            CREATE TABLE IF NOT EXISTS public.stock_prices (
                id UUID PRIMARY KEY,
                ticker VARCHAR(10) NOT NULL,
                price NUMERIC(12, 6) NOT NULL,
                timestamp TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'UTC')
            );

            -- Create an index on the ticker column for faster lookups
            CREATE INDEX IF NOT EXISTS idx_stock_prices_ticker ON public.stock_prices(ticker);
            
            -- Create an index on the timestamp column for faster time-based queries
            CREATE INDEX IF NOT EXISTS idx_stock_prices_timestamp ON public.stock_prices(timestamp);

            -- Check if transactions table exists, if not, create it.
            CREATE TABLE IF NOT EXISTS public.transactions (
                id UUID PRIMARY KEY,
                user_id UUID NOT NULL,
                ticker VARCHAR(10) NOT NULL,
                limit_price DECIMAL(18, 2) NOT NULL,
                transaction_type INT NOT NULL CHECK (transaction_type IN (0, 1)),
                quantity INT NOT NULL CHECK (quantity > 0),
                created_on_utc TIMESTAMP WITHOUT TIME ZONE DEFAULT (NOW() AT TIME ZONE 'UTC'),

                CONSTRAINT fk_transactions_users FOREIGN KEY (user_id)
                REFERENCES public.users (id) ON DELETE CASCADE
            );

            -- Create an index on the ticker column for faster lookups
            CREATE INDEX IF NOT EXISTS idx_transactions_ticker ON public.transactions(ticker);

            -- Check if budgets table exists, if not, create it.
            CREATE TABLE IF NOT EXISTS public.budgets (
                id UUID PRIMARY KEY,
                user_id UUID NOT NULL,
                buying_power DECIMAL(18, 2) NOT NULL,

                CONSTRAINT fk_transactions_users FOREIGN KEY (user_id)
                REFERENCES public.users (id) ON DELETE CASCADE
            );

            -- Check if roles table exists, if not, create it.
            CREATE TABLE IF NOT EXISTS public.roles (
                id SERIAL PRIMARY KEY,
                name TEXT NOT NULL UNIQUE
            );

            -- Insert Admin and Member roles if they do not already exist.
            INSERT INTO public.roles (id, name)
            VALUES 
                (1, 'Admin'),
                (2, 'Member')
            ON CONFLICT (id) DO NOTHING;

            -- Create the user_roles table if it doesn't exist
            CREATE TABLE IF NOT EXISTS public.user_roles (
                user_id UUID NOT NULL,
                role_id INT NOT NULL,

                PRIMARY KEY (user_id, role_id),
                FOREIGN KEY (user_id) REFERENCES public.users(id) ON DELETE CASCADE,
                FOREIGN KEY (role_id) REFERENCES public.roles(id) ON DELETE CASCADE
            );
            """;

        await connection.ExecuteAsync(sql);
    }
}
