using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Repositories;
using Modules.Users.Domain.Services;
using Modules.Users.Domain.ValueObjects;
using NSubstitute;
using SharedKernel;

namespace Domain.UnitTests.Followers;

public sealed class FollowerServiceTests
{
    private readonly IFollowerRepository _followerRepositoryMock;
    private readonly FollowerService _followerService;
    private static readonly Email Email = Email.Create("test@test.com").Value;
    private static readonly Username Username = Username.Create("Username").Value;
    private static readonly DateTime UtcNow = DateTime.UtcNow;

    public FollowerServiceTests()
    {
        _followerRepositoryMock = Substitute.For<IFollowerRepository>();

        IDateTimeProvider dateTimeProviderMock = Substitute.For<IDateTimeProvider>();
        dateTimeProviderMock.UtcNow.Returns(UtcNow);

        _followerService = new FollowerService(_followerRepositoryMock, dateTimeProviderMock);
    }

    [Fact]
    public async Task StartFollowingAsync_Should_ReturnError_WhenFollowingSameUser()
    {
        // Arrange
        var user = User.Create(Username, Email, "password", "link", hasPublicProfile: false);

        // Act
        Result<Follower> result = await _followerService.StartFollowingAsync(user, user, default);

        // Assert
        Assert.Equal(FollowerErrors.SameUser, result.Error);
    }

    [Fact]
    public async Task StartFollowingAsync_Should_ReturnError_WhenFollowingNonPublicProfile()
    {
        // Arrange
        var user = User.Create(Username, Email, "password", "link", hasPublicProfile: true);
        var followed = User.Create(Username, Email, "password", "link", hasPublicProfile: false);

        // Act
        Result<Follower> result = await _followerService.StartFollowingAsync(user, user, default);

        // Assert
        Assert.Equal(FollowerErrors.SameUser, result.Error);
    }

    [Fact]
    public async Task StartFollowingAsync_Should_ReturnError_WhenAlreadyFollowing()
    {
        // Arrange
        var user = User.Create(Username, Email, "password", "link", hasPublicProfile: true);
        var followed = User.Create(Username, Email, "password", "link", hasPublicProfile: true);

        _followerRepositoryMock
           .IsAlreadyFollowingAsync(user.Id, followed.Id)
           .Returns(true);

        // Act
        Result<Follower> result = await _followerService.StartFollowingAsync(user, followed, default);

        // Assert
        Assert.Equal(FollowerErrors.AlreadyFollowing, result.Error);
    }

    [Fact]
    public async Task StartFollowingAsync_Should_ReturnSuccess_WhenFollowerCreated()
    {
        // Arrange
        var user = User.Create(Username, Email, "password", "link", hasPublicProfile: true);
        var followed = User.Create(Username, Email, "password", "link", hasPublicProfile: true);

        _followerRepositoryMock
            .IsAlreadyFollowingAsync(user.Id, followed.Id)
            .Returns(false);

        // Act
        Result<Follower> result = await _followerService.StartFollowingAsync(user, followed, default);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task StartFollowingAsync_Should_ReturnFollower_WhenFollowerCreated()
    {
        // Arrange
        var user = User.Create(Username, Email, "password", "link", hasPublicProfile: true);
        var followed = User.Create(Username, Email, "password", "link", hasPublicProfile: true);

        _followerRepositoryMock
            .IsAlreadyFollowingAsync(user.Id, followed.Id)
            .Returns(false);

        // Act
        Result<Follower> result = await _followerService.StartFollowingAsync(user, followed, default);

        // Assert
        Follower follower = result.Value;

        Assert.Equal(user.Id, follower.UserId);
        Assert.Equal(followed.Id, follower.FollowedId);
        Assert.Equal(UtcNow, follower.CreatedOnUtc);
    }
}
