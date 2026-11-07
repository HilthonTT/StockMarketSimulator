using System.Data;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Api.FunctionalTests.Abstractions;
using Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Users.BackgroundJobs.Users.RevokeExpiredRefreshTokens;
using Modules.Users.Contracts.Users;
using Modules.Users.Infrastructure.Database;

namespace Api.FunctionalTests.Users;

public sealed class RefreshTokenTests : BaseFunctionalTest
{
    public RefreshTokenTests(FunctionalTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Login_Should_StoreOnlyTheHashOfTheRefreshToken()
    {
        // Act
        (Guid userId, TokenResponse tokens) = await RegisterAndLoginAsync();

        // Assert
        List<string> storedHashes = await GetStoredHashesAsync(userId);

        string storedHash = Assert.Single(storedHashes);
        Assert.Equal(Sha256Hex(tokens.RefreshToken), storedHash);
        Assert.NotEqual(tokens.RefreshToken, storedHash);
    }

    [Fact]
    public async Task RefreshTokens_Should_RotateToken_AndRejectThePreviousOne()
    {
        // Arrange
        (Guid userId, TokenResponse tokens) = await RegisterAndLoginAsync();

        // Act
        HttpResponseMessage refreshResponse = await Client.PostAsJsonAsync(
            "api/v1/authentication/refresh-tokens",
            new LoginWithRefreshTokenRequest(tokens.RefreshToken));

        // Assert
        refreshResponse.EnsureSuccessStatusCode();
        TokenResponse rotated = (await refreshResponse.Content.ReadFromJsonAsync<TokenResponse>())!;

        Assert.NotEqual(tokens.RefreshToken, rotated.RefreshToken);
        Assert.Equal(Sha256Hex(rotated.RefreshToken), Assert.Single(await GetStoredHashesAsync(userId)));

        HttpResponseMessage reuseResponse = await Client.PostAsJsonAsync(
            "api/v1/authentication/refresh-tokens",
            new LoginWithRefreshTokenRequest(tokens.RefreshToken));

        Assert.False(reuseResponse.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Cleanup_Should_KeepUnexpiredTokensOfOtherDevices()
    {
        // Arrange: two sessions (devices) for the same user.
        (Guid userId, TokenResponse firstDevice) = await RegisterAndLoginAsync();
        string email = (await GetUserByIdAsync(userId))!.Email.Value;

        HttpResponseMessage secondLogin = await Client.PostAsJsonAsync(
            "api/v1/authentication/login",
            new LoginRequest(email, Password));
        secondLogin.EnsureSuccessStatusCode();

        // Act
        await RunCleanupAsync(DateTime.UtcNow);

        // Assert: neither session was revoked.
        Assert.Equal(2, (await GetStoredHashesAsync(userId)).Count);

        HttpResponseMessage refreshFirstDevice = await Client.PostAsJsonAsync(
            "api/v1/authentication/refresh-tokens",
            new LoginWithRefreshTokenRequest(firstDevice.RefreshToken));
        Assert.True(refreshFirstDevice.IsSuccessStatusCode);
    }

    [Fact]
    public async Task Cleanup_Should_DeleteExpiredTokens()
    {
        // Arrange
        (Guid userId, _) = await RegisterAndLoginAsync();

        // Act: pretend the cleanup runs after the token's absolute expiry (7 days).
        await RunCleanupAsync(DateTime.UtcNow.AddDays(8));

        // Assert
        Assert.Empty(await GetStoredHashesAsync(userId));
    }

    private async Task RunCleanupAsync(DateTime utcNow)
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        IDbConnectionFactory connectionFactory = scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>();

        using IDbConnection connection = await connectionFactory.GetOpenConnectionAsync();

        await RevokeExpiredRefreshTokenJob.CleanUpAsync(connection, utcNow);
    }

    private async Task<List<string>> GetStoredHashesAsync(Guid userId)
    {
        using IServiceScope scope = Factory.Services.CreateScope();
        UsersDbContext dbContext = scope.ServiceProvider.GetRequiredService<UsersDbContext>();

        return await dbContext.RefreshTokens
            .AsNoTracking()
            .Where(token => token.UserId == userId)
            .Select(token => token.TokenHash)
            .ToListAsync();
    }

    private static string Sha256Hex(string value) =>
        Convert.ToHexStringLower(SHA256.HashData(Encoding.UTF8.GetBytes(value)));
}
