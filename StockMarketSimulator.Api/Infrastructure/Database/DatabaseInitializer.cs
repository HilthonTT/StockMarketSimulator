using Dapper;
using Npgsql;
using Quartz;
using SharedKernel;

namespace StockMarketSimulator.Api.Infrastructure.Database;

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

        const string tableSql =
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

        const string quartzSql =
            """
            set client_min_messages = WARNING;
            DROP TABLE IF EXISTS scheduler.qrtz_fired_triggers;
            DROP TABLE IF EXISTS scheduler.qrtz_paused_trigger_grps;
            DROP TABLE IF EXISTS scheduler.qrtz_scheduler_state;
            DROP TABLE IF EXISTS scheduler.qrtz_locks;
            DROP TABLE IF EXISTS scheduler.qrtz_simprop_triggers;
            DROP TABLE IF EXISTS scheduler.qrtz_simple_triggers;
            DROP TABLE IF EXISTS scheduler.qrtz_cron_triggers;
            DROP TABLE IF EXISTS scheduler.qrtz_blob_triggers;
            DROP TABLE IF EXISTS scheduler.qrtz_triggers;
            DROP TABLE IF EXISTS scheduler.qrtz_job_details;
            DROP TABLE IF EXISTS scheduler.qrtz_calendars;
            set client_min_messages = NOTICE;

            CREATE SCHEMA IF NOT EXISTS scheduler;

            CREATE TABLE scheduler.qrtz_job_details
                (
                sched_name TEXT NOT NULL,
                job_name  TEXT NOT NULL,
                job_group TEXT NOT NULL,
                description TEXT NULL,
                job_class_name   TEXT NOT NULL, 
                is_durable BOOL NOT NULL,
                is_nonconcurrent BOOL NOT NULL,
                is_update_data BOOL NOT NULL,
                requests_recovery BOOL NOT NULL,
                job_data BYTEA NULL,
                PRIMARY KEY (sched_name,job_name,job_group)
            );

            CREATE TABLE scheduler.qrtz_triggers
                (
                sched_name TEXT NOT NULL,
                trigger_name TEXT NOT NULL,
                trigger_group TEXT NOT NULL,
                job_name  TEXT NOT NULL, 
                job_group TEXT NOT NULL,
                description TEXT NULL,
                next_fire_time BIGINT NULL,
                prev_fire_time BIGINT NULL,
                priority INTEGER NULL,
                trigger_state TEXT NOT NULL,
                trigger_type TEXT NOT NULL,
                start_time BIGINT NOT NULL,
                end_time BIGINT NULL,
                calendar_name TEXT NULL,
                misfire_instr SMALLINT NULL,
                job_data BYTEA NULL,
                PRIMARY KEY (sched_name,trigger_name,trigger_group),
                FOREIGN KEY (sched_name,job_name,job_group) 
                    REFERENCES scheduler.qrtz_job_details(sched_name,job_name,job_group) 
            );

            CREATE TABLE scheduler.qrtz_simple_triggers
                (
                sched_name TEXT NOT NULL,
                trigger_name TEXT NOT NULL,
                trigger_group TEXT NOT NULL,
                repeat_count BIGINT NOT NULL,
                repeat_interval BIGINT NOT NULL,
                times_triggered BIGINT NOT NULL,
                PRIMARY KEY (sched_name,trigger_name,trigger_group),
                FOREIGN KEY (sched_name,trigger_name,trigger_group) 
                    REFERENCES scheduler.qrtz_triggers(sched_name,trigger_name,trigger_group) ON DELETE CASCADE
            );

            CREATE TABLE scheduler.qrtz_simprop_triggers 
                (
                sched_name TEXT NOT NULL,
                trigger_name TEXT NOT NULL ,
                trigger_group TEXT NOT NULL ,
                str_prop_1 TEXT NULL,
                str_prop_2 TEXT NULL,
                str_prop_3 TEXT NULL,
                int_prop_1 INTEGER NULL,
                int_prop_2 INTEGER NULL,
                long_prop_1 BIGINT NULL,
                long_prop_2 BIGINT NULL,
                dec_prop_1 NUMERIC NULL,
                dec_prop_2 NUMERIC NULL,
                bool_prop_1 BOOL NULL,
                bool_prop_2 BOOL NULL,
                time_zone_id TEXT NULL,
                PRIMARY KEY (sched_name,trigger_name,trigger_group),
                FOREIGN KEY (sched_name,trigger_name,trigger_group) 
                    REFERENCES scheduler.qrtz_triggers(sched_name,trigger_name,trigger_group) ON DELETE CASCADE
            );

            CREATE TABLE scheduler.qrtz_cron_triggers
                (
                sched_name TEXT NOT NULL,
                trigger_name TEXT NOT NULL,
                trigger_group TEXT NOT NULL,
                cron_expression TEXT NOT NULL,
                time_zone_id TEXT,
                PRIMARY KEY (sched_name,trigger_name,trigger_group),
                FOREIGN KEY (sched_name,trigger_name,trigger_group) 
                    REFERENCES scheduler.qrtz_triggers(sched_name,trigger_name,trigger_group) ON DELETE CASCADE
            );

            CREATE TABLE scheduler.qrtz_blob_triggers
                (
                sched_name TEXT NOT NULL,
                trigger_name TEXT NOT NULL,
                trigger_group TEXT NOT NULL,
                blob_data BYTEA NULL,
                PRIMARY KEY (sched_name,trigger_name,trigger_group),
                FOREIGN KEY (sched_name,trigger_name,trigger_group) 
                    REFERENCES scheduler.qrtz_triggers(sched_name,trigger_name,trigger_group) ON DELETE CASCADE
            );

            CREATE TABLE scheduler.qrtz_calendars
                (
                sched_name TEXT NOT NULL,
                calendar_name  TEXT NOT NULL, 
                calendar BYTEA NOT NULL,
                PRIMARY KEY (sched_name,calendar_name)
            );

            CREATE TABLE scheduler.qrtz_paused_trigger_grps
                (
                sched_name TEXT NOT NULL,
                trigger_group TEXT NOT NULL, 
                PRIMARY KEY (sched_name,trigger_group)
            );

            CREATE TABLE scheduler.qrtz_fired_triggers 
                (
                sched_name TEXT NOT NULL,
                entry_id TEXT NOT NULL,
                trigger_name TEXT NOT NULL,
                trigger_group TEXT NOT NULL,
                instance_name TEXT NOT NULL,
                fired_time BIGINT NOT NULL,
                sched_time BIGINT NOT NULL,
                priority INTEGER NOT NULL,
                state TEXT NOT NULL,
                job_name TEXT NULL,
                job_group TEXT NULL,
                is_nonconcurrent BOOL NOT NULL,
                requests_recovery BOOL NULL,
                PRIMARY KEY (sched_name,entry_id)
            );

            CREATE TABLE scheduler.qrtz_scheduler_state 
                (
                sched_name TEXT NOT NULL,
                instance_name TEXT NOT NULL,
                last_checkin_time BIGINT NOT NULL,
                checkin_interval BIGINT NOT NULL,
                PRIMARY KEY (sched_name,instance_name)
            );

            CREATE TABLE scheduler.qrtz_locks
                (
                sched_name TEXT NOT NULL,
                lock_name  TEXT NOT NULL, 
                PRIMARY KEY (sched_name,lock_name)
            );

            CREATE INDEX idx_qrtz_j_req_recovery ON scheduler.qrtz_job_details(requests_recovery);
            CREATE INDEX idx_qrtz_t_next_fire_time ON scheduler.qrtz_triggers(next_fire_time);
            CREATE INDEX idx_qrtz_t_state ON scheduler.qrtz_triggers(trigger_state);
            CREATE INDEX idx_qrtz_t_nft_st ON scheduler.qrtz_triggers(next_fire_time,trigger_state);
            CREATE INDEX idx_qrtz_ft_trig_name ON scheduler.qrtz_fired_triggers(trigger_name);
            CREATE INDEX idx_qrtz_ft_trig_group ON scheduler.qrtz_fired_triggers(trigger_group);
            CREATE INDEX idx_qrtz_ft_trig_nm_gp ON scheduler.qrtz_fired_triggers(sched_name,trigger_name,trigger_group);
            CREATE INDEX idx_qrtz_ft_trig_inst_name ON scheduler.qrtz_fired_triggers(instance_name);
            CREATE INDEX idx_qrtz_ft_job_name ON scheduler.qrtz_fired_triggers(job_name);
            CREATE INDEX idx_qrtz_ft_job_group ON scheduler.qrtz_fired_triggers(job_group);
            CREATE INDEX idx_qrtz_ft_job_req_recovery ON scheduler.qrtz_fired_triggers(requests_recovery);
            """;

        await connection.ExecuteAsync(tableSql);
        await connection.ExecuteAsync(quartzSql);
    }
}
