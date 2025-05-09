﻿using System.Data;
using System.Security.Cryptography;
using Application.Abstractions.Caching;
using Application.Abstractions.Data;
using Dapper;
using Microsoft.Extensions.Logging;
using Modules.Stocks.Application.Abstractions.Url;
using Modules.Stocks.Domain.Entities;
using Npgsql;
using SharedKernel;

namespace Modules.Stocks.Infrastructure.Url;

internal sealed class UrlShorteningService(
    IDbConnectionFactory dbConnectionFactory,
    ICacheService cacheService,
    ILogger<UrlShorteningService> logger,
    IDateTimeProvider dateTimeProvider) : IUrlShorteningService
{
    private const int MaxRetries = 3;

    public async Task<string?> GetOriginalUrlAsync(string shortCode, CancellationToken cancellationToken = default)
    {
        string? originalUrl = await cacheService.GetAsync<string>(shortCode, cancellationToken);
        if (!string.IsNullOrWhiteSpace(originalUrl))
        {
            return originalUrl;
        }

        const string sql =
            """
            SELECT original_url
            FROM stocks.shortened_urls
            WHERE short_code = @ShortCode;
            """;

        using IDbConnection connection = await dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        originalUrl = await connection.QuerySingleOrDefaultAsync<string>(
            sql,
            new { ShortCode = shortCode });

        await cacheService.SetAsync(shortCode, originalUrl, cancellationToken: cancellationToken);

        return originalUrl;
    }

    public async Task<string> ShortenUrlAsync(string originalUrl, CancellationToken cancellationToken = default)
    {
        using IDbConnection connection = await dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                string shortCode = GenerateShortCode();

                const string sql =
                    """
                    INSERT INTO stocks.shortened_urls (id, short_code, original_url, created_on_utc)
                    VALUES (@Id, @ShortCode, @OriginalUrl, @CreatedOnUtc)
                    RETURNING short_code;
                    """;

                string result = await connection.QuerySingleAsync<string>(
                    sql,
                    new
                    {
                        Id = Guid.CreateVersion7(),
                        ShortCode = shortCode,
                        OriginalUrl = originalUrl,
                        CreatedOnUtc = dateTimeProvider.UtcNow,
                    });

                await cacheService.SetAsync(shortCode, originalUrl, cancellationToken: cancellationToken);

                return result;
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                if (attempt == MaxRetries)
                {
                    logger.LogError(ex, "Failed to generate unique short code after {MaxRetries} attempts", MaxRetries);

                    throw new InvalidOperationException("Failed to generate unique short code", ex);
                }

                logger.LogWarning(
                    "Short code collision occured. Retrying... (Attempt {Attempt} of {MaxRetries})",
                    attempt + 1,
                    MaxRetries);
            }
        }

        throw new InvalidOperationException("Failed to generate unique short code");
    }

    public async Task<string> ShortenUrlAsync(string shortCode, string originalUrl, CancellationToken cancellationToken = default)
    {
        using IDbConnection connection = await dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            try
            {
                const string sql =
                    """
                    INSERT INTO stocks.shortened_urls (id, short_code, original_url, created_on_utc)
                    VALUES (@Id, @ShortCode, @OriginalUrl, @CreatedOnUtc)
                    RETURNING short_code;
                    """;

                string result = await connection.QuerySingleAsync<string>(
                    sql,
                    new
                    {
                        Id = Guid.CreateVersion7(),
                        ShortCode = shortCode,
                        OriginalUrl = originalUrl,
                        CreatedOnUtc = dateTimeProvider.UtcNow,
                    });

                await cacheService.SetAsync(shortCode, originalUrl, cancellationToken: cancellationToken);

                return result;
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation)
            {
                if (attempt == MaxRetries)
                {
                    logger.LogError(ex, "Failed to generate unique short code after {MaxRetries} attempts", MaxRetries);

                    throw new InvalidOperationException("Failed to generate unique short code", ex);
                }

                logger.LogWarning(
                    "Short code collision occured. Retrying... (Attempt {Attempt} of {MaxRetries})",
                    attempt + 1,
                    MaxRetries);
            }
        }

        throw new InvalidOperationException("Failed to generate unique short code");
    }

    public async Task<IEnumerable<ShortenedUrl>> GetAllUrlsAsync(CancellationToken cancellationToken = default)
    {
        const string sql =
            """
            SELECT 
                id AS Id,
                short_code AS ShortCode,
                original_url AS OriginalUrl,
                created_on_utc AS CreatedOnUtc,
                modified_on_utc AS ModifiedOnUtc
            FROM stocks.shortened_urls
            ORDER BY created_on_utc DESC;
            """;

        using IDbConnection connection = await dbConnectionFactory.GetOpenConnectionAsync(cancellationToken);

        return await connection.QueryAsync<ShortenedUrl>(sql);
    }

    private static string GenerateShortCode()
    {
        const int length = 8;
        const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        var bytes = new byte[length];
        RandomNumberGenerator.Fill(bytes);

        char[] chars = new char[length];

        for (int i = 0; i < length; i++)
        {
            chars[i] = alphabet[bytes[i] % alphabet.Length];
        }

        return new string(chars);
    }
}
