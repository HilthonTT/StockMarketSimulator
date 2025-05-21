using System.Data;
using System.Text.Json;
using Application.Abstractions.Caching;
using Application.Abstractions.Data;
using Dapper;
using Infrastructure.Database;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Caching;

internal sealed class PostgresCacheService(
    IDbConnectionFactory dbConnectionFactory,
    ILogger<PostgresCacheService> logger) : ICacheService
{
    public async Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default)
    {
        using IDbConnection connection = await dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        const string sql =
            $"""
            SELECT value
            FROM {Schemas.General}.cache
            WHERE key = @Key;
            """;

        string? jsonString = await connection.QueryFirstOrDefaultAsync<string>(
            new CommandDefinition(sql, new { Key = key }, cancellationToken: cancellationToken));

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
                "Error deserializing JSON for key {Key} to type {Type}: {ErrorMessage}", key, typeof(T).Name, ex.Message);
            return default;
        }
    }

    public async Task<T> GetOrCreateAsync<T>(
        string key,
        Func<Task<T>> factory, 
        TimeSpan? expiration = null, 
        CancellationToken cancellationToken = default)
    {
        var cachedValue = await GetAsync<T>(key, cancellationToken);

        if (cachedValue is not null)
        {
            return cachedValue;
        }

        var newValue = await factory();
        await SetAsync(key, newValue, expiration, cancellationToken);

        return newValue;
    }

    public async Task SetAsync<T>(
        string key, 
        T value, 
        TimeSpan? expiration = null, 
        CancellationToken cancellationToken = default)
    {
        using IDbConnection connection = await dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        string jsonString;
        try
        {
            jsonString = JsonSerializer.Serialize(value);
        }
        catch (JsonException ex)
        {
            logger.LogError(
                "Error serializing JSON for key {Key} to type {Type}: {ErrorMessage}", key, typeof(T).Name, ex.Message);
            throw;
        }

        const string sql =
            $"""
            INSERT INTO {Schemas.General}.cache(key, value)
            VALUES (@Key, @Value::jsonb)
            ON CONFLICT (key) DO UPDATE
            SET VALUE = excluded.value;
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(sql, new { Key = key, Value = jsonString }, cancellationToken: cancellationToken));
    }

    public async Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        using IDbConnection connection = await dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        const string sql =
            $"""
            DELETE FROM ${Schemas.General}.cache 
            WHERE key = @Key;
            """;

        await connection.ExecuteAsync(
            new CommandDefinition(sql, new { Key = key }, cancellationToken: cancellationToken));
    }
}
