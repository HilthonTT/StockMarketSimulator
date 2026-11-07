using System.Data;
using Application.Abstractions.Data;
using Application.Abstractions.Messaging;
using Application.IntegrationTests.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Modules.Users.Application.Authentication.Register;
using Modules.Users.BackgroundJobs.Users.RevokeExpiredRefreshTokens;
using Modules.Users.Domain.Entities;
using SharedKernel;

namespace Application.IntegrationTests.Users;

public sealed class RevokeExpiredRefreshTokenJobTests : BaseIntegrationTest
{
    public RevokeExpiredRefreshTokenJobTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CleanUp_Should_KeepAllUnexpiredTokens_UpToTheCap()
    {
        // Arrange
        Guid userId = await RegisterUserAsync();
        await AddTokensAsync(userId, 3);

        // Act
        await CleanUpAsync(DateTime.UtcNow);

        // Assert
        Assert.Equal(3, await CountTokensAsync(userId));
    }

    [Fact]
    public async Task CleanUp_Should_DeleteOnlyTokensOverThePerUserCap()
    {
        // Arrange
        Guid userId = await RegisterUserAsync();
        Guid otherUserId = await RegisterUserAsync();
        await AddTokensAsync(userId, RevokeExpiredRefreshTokenJob.MaxTokensPerUser + 2);
        await AddTokensAsync(otherUserId, 1);

        // Act
        await CleanUpAsync(DateTime.UtcNow);

        // Assert
        Assert.Equal(RevokeExpiredRefreshTokenJob.MaxTokensPerUser, await CountTokensAsync(userId));
        Assert.Equal(1, await CountTokensAsync(otherUserId));
    }

    [Fact]
    public async Task CleanUp_Should_DeleteExpiredTokens()
    {
        // Arrange
        Guid userId = await RegisterUserAsync();
        await AddTokensAsync(userId, 2);

        // Act: refresh tokens expire after 7 days.
        await CleanUpAsync(DateTime.UtcNow.AddDays(8));

        // Assert
        Assert.Equal(0, await CountTokensAsync(userId));
    }

    private async Task<Guid> RegisterUserAsync()
    {
        var handler = ScopedServices.GetRequiredService<ICommandHandler<RegisterUserCommand, Guid>>();

        Result<Guid> result = await handler.Handle(
            new RegisterUserCommand(Faker.Internet.Email(), Faker.Internet.UserName(), "Passw0rd!" + Faker.Random.AlphaNumeric(6)),
            default);

        Assert.True(result.IsSuccess, result.IsFailure ? $"{result.Error.Code}: {result.Error.Description}" : null);

        return result.Value;
    }

    private async Task AddTokensAsync(Guid userId, int count)
    {
        for (int i = 0; i < count; i++)
        {
            UsersDbContext.RefreshTokens.Add(RefreshToken.Create(Guid.NewGuid().ToString("N") + Guid.NewGuid().ToString("N"), userId));
        }

        await UsersDbContext.SaveChangesAsync();
    }

    private async Task CleanUpAsync(DateTime utcNow)
    {
        IDbConnectionFactory connectionFactory = ScopedServices.GetRequiredService<IDbConnectionFactory>();

        using IDbConnection connection = await connectionFactory.GetOpenConnectionAsync();

        await RevokeExpiredRefreshTokenJob.CleanUpAsync(connection, utcNow);
    }

    private Task<int> CountTokensAsync(Guid userId) =>
        UsersDbContext.RefreshTokens.AsNoTracking().CountAsync(token => token.UserId == userId);
}
