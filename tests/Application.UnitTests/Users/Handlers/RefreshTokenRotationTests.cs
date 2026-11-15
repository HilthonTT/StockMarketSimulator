using Microsoft.EntityFrameworkCore;
using Modules.Users.Application.Abstractions.Authentication;
using Modules.Users.Application.Abstractions.Data;
using Modules.Users.Application.Authentication.LoginWithRefreshToken;
using Modules.Users.Contracts.Users;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Repositories;
using Modules.Users.Infrastructure.Authentication;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using SharedKernel;

namespace Application.UnitTests.Users.Handlers;

public sealed class RefreshTokenRotationTests
{
    [Fact]
    public async Task LoginWithRefreshToken_Should_Fail_WhenAConcurrentRequestRotatedTheTokenFirst()
    {
        // Arrange
        var repository = Substitute.For<IRefreshTokenRepository>();
        var tokenProvider = Substitute.For<ITokenProvider>();
        var unitOfWork = Substitute.For<IUnitOfWork>();

        tokenProvider.HashRefreshToken(Arg.Any<string>()).Returns(call => RefreshTokenHasher.Hash(call.Arg<string>()));
        tokenProvider.GenerateRefreshToken().Returns("new-plain-token");
        tokenProvider.Create(Arg.Any<User>()).Returns("access-token");

        var stored = RefreshToken.Create(RefreshTokenHasher.Hash("old-plain-token"), Guid.CreateVersion7());
        repository.GetByTokenHashAsync(Arg.Any<string>(), Arg.Any<CancellationToken>())
            .Returns(Option<RefreshToken>.Some(stored));

        // token_hash is a concurrency token: the losing UPDATE matches no row.
        unitOfWork.SaveChangesAsync(Arg.Any<CancellationToken>())
            .ThrowsAsync(new DbUpdateConcurrencyException("token already rotated"));

        var handler = new LoginUserWithRefreshTokenCommandHandler(repository, tokenProvider, unitOfWork);

        // Act
        Result<TokenResponse> result = await handler.Handle(new LoginUserWithRefreshTokenCommand("old-plain-token"), default);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(RefreshTokenErrors.Expired, result.Error);
    }
}
