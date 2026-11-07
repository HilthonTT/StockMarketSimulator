using System.Data;
using System.Text.Json;
using Application.Abstractions.Caching;
using Application.Abstractions.Data;
using Dapper;
using Infrastructure.Database;
using Microsoft.Extensions.Logging;
using SharedKernel;

namespace Infrastructure.Caching;

/// <summary>
/// Fallback <see cref="ICacheService"/> used when Redis is unreachable at startup.
/// Stores entries in the unlogged <c>general.cache_items</c> table.
/// </summary>
internal sealed class PostgresCacheService(
    IDbConnectionFactory dbConnectionFactory,
    IDateTimeProvider dateTimeProvider,
    ILogger<PostgresCacheService> logger) : ICacheService
{
    private const string Table = $"{Schemas.General}.cache_items";

    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        using IDbConnection connection = await dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        const string sql =
            $"""
            SELECT value::text
            FROM {Table}
            WHERE key = @Key
              AND (expires_on_utc IS NULL OR expires_on_utc > @UtcNow);
            """;

        string? jsonString = await connection.QueryFirstOrDefaultAsync<string>(
            new CommandDefinition(
                sql,
                new { Key = key, dateTimeProvider.UtcNow },
                cancellationToken: cancellationToken));

        if (string.IsNullOrWhiteSpace(jsonString))
        {
            return default;
        }

        try
        {
            return JsonSerializer.Deserialize<T>(jsonString);
        }
        catch (JsonException ex)
        {
            logger.LogError(
                ex,
                "Error deserializing JSON for key {Key} to type {Type}",
                key,
                typeof(T).Name);

            return default;
        }
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        T? cachedValue = await GetAsync<T>(key, cancellationToken);

        if (cachedValue is not null)
        {
            return cachedValue;
        }

        T newValue = await factory();

        await SetAsync(key, newValue, expiration, cancellationToken);

        return newValue;
    }

    public async Task SetAsync<T>(
        string key,
        T value,
        TimeSpan? expiration = null,
        CancellationToken cancellationToken = default)
    {
        string jsonString = JsonSerializer.Serialize(value);

        DateTime expiresOnUtc = dateTimeProvider.UtcNow.Add(
            expiration ?? CacheOptions.DefaultExpiration.AbsoluteExpirationRelativeToNow!.Value);

        using IDbConnection connection = await dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        const string sql =
            $"""
            INSERT INTO {Table} (key, value, expires_on_utc)
            VALUES (@Key, @Value::jsonb, @ExpiresOnUtc)
            ON CONFLICT (key) DO UPDATE
            SET value = excluded.value,
                expires_on_utc = excluded.expires_on_utc;
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new { Key = key, Value = jsonString, ExpiresOnUtc = expiresOnUtc },
                cancellationToken: cancellationToken));
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        using IDbConnection connection = await dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        const string sql =
            $"""
            DELETE FROM {Table}
            WHERE key = @Key;
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(sql, new { Key = key }, cancellationToken: cancellationToken));
    }
}
