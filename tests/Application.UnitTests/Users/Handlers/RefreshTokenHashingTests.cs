using Modules.Users.Application.Abstractions.Authentication;
using Modules.Users.Application.Abstractions.Data;
using Modules.Users.Application.Authentication.LoginWithRefreshToken;
using Modules.Users.Contracts.Users;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Repositories;
using Modules.Users.Infrastructure.Authentication;
using NSubstitute;
using SharedKernel;

namespace Application.UnitTests.Users.Handlers;

public sealed class RefreshTokenHashingTests
{
    [Fact]
    public void Hash_Should_BeDeterministicLowerCaseSha256Hex()
    {
        string hash = RefreshTokenHasher.Hash("plain-token");

        Assert.Equal(RefreshTokenHasher.Hash("plain-token"), hash);
        Assert.Equal(64, hash.Length);
        Assert.Matches("^[0-9a-f]{64}$", hash);
        Assert.NotEqual(RefreshTokenHasher.Hash("other-token"), hash);

        // Known SHA-256 test vector for "abc".
        Assert.Equal(
            "ba7816bf8f01cfea414140de5dae2223b00361a396177a9cb410ff61f20015ad",
            RefreshTokenHasher.Hash("abc"));
    }

    [Fact]
    public async Task LoginWithRefreshToken_Should_LookUpByHash_AndStoreOnlyTheNewHash()
    {
        // Arrange
        var repository = Substitute.For<IRefreshTokenRepository>();
        var tokenProvider = Substitute.For<ITokenProvider>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        tokenProvider.HashRefreshToken(Arg.Any<string>()).Returns(call => RefreshTokenHasher.Hash(call.Arg<string>()));
        tokenProvider.GenerateRefreshToken().Returns("new-plain-token");
        tokenProvider.Create(Arg.Any<User>()).Returns("access-token");

        var stored = RefreshToken.Create(RefreshTokenHasher.Hash("old-plain-token"), Guid.CreateVersion7());
        repository.GetByTokenHashAsync(RefreshTokenHasher.Hash("old-plain-token"), Arg.Any<CancellationToken>())
            .Returns(Option<RefreshToken>.Some(stored));

        var handler = new LoginUserWithRefreshTokenCommandHandler(repository, tokenProvider, unitOfWork);

        // Act
        Result<TokenResponse> result = await handler.Handle(new LoginUserWithRefreshTokenCommand("old-plain-token"), default);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal("new-plain-token", result.Value.RefreshToken);
        Assert.Equal(RefreshTokenHasher.Hash("new-plain-token"), stored.TokenHash);
        Assert.NotEqual("new-plain-token", stored.TokenHash);
    }

    [Fact]
    public async Task LoginWithRefreshToken_Should_Fail_WhenNoTokenMatchesTheHash()
    {
        var repository = Substitute.For<IRefreshTokenRepository>();
        var tokenProvider = Substitute.For<ITokenProvider>();
        tokenProvider.HashRefreshToken(Arg.Any<string>()).Returns(call => RefreshTokenHasher.Hash(call.Arg<string>()));
        repository.GetByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Option<RefreshToken>.None());

        var handler = new LoginUserWithRefreshTokenCommandHandler(repository, tokenProvider, Substitute.For<IUnitOfWork>());

        Result<TokenResponse> result = await handler.Handle(new LoginUserWithRefreshTokenCommand("unknown"), default);

        Assert.True(result.IsFailure);
    }
}
