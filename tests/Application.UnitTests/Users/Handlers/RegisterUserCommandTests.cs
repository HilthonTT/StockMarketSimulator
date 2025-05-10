using Modules.Users.Application.Abstractions.Data;
using Modules.Users.Application.Abstractions.Factories;
using Modules.Users.Application.Authentication.Register;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Errors;
using Modules.Users.Domain.Repositories;
using Modules.Users.Domain.ValueObjects;
using NSubstitute;
using SharedKernel;

namespace Application.UnitTests.Users.Handlers;

public sealed class RegisterUserCommandTests
{
    private static readonly RegisterUserCommand Command = new("test@test.com", "Username", "Password");

    private readonly RegisterUserCommandHandler _handler;
    private readonly IUserRepository _userRepositoryMock;
    private readonly IUserFactory _userFactoryMock;
    private readonly IEmailVerificationTokenRepository _emailVerificationTokenRepositoryMock;
    private readonly IUnitOfWork _unitOfWorkMock;

    public RegisterUserCommandTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _userFactoryMock = Substitute.For<IUserFactory>();
        _emailVerificationTokenRepositoryMock = Substitute.For<IEmailVerificationTokenRepository>();
        _unitOfWorkMock = Substitute.For<IUnitOfWork>();

        _handler = new RegisterUserCommandHandler(
            _userFactoryMock,
            _userRepositoryMock,
            _emailVerificationTokenRepositoryMock,
            _unitOfWorkMock);
    }

    [Fact]
    public async Task Handle_Should_ReturnError_WhenEmailIsInvalid()
    {
        // Arrange
        RegisterUserCommand invalidCommand = Command with { Email = "invalid_email" };

        _userFactoryMock.CreateAsync(Arg.Any<RegisterUserCommand>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<User>(EmailErrors.InvalidFormat));

        // Act
        Result<Guid> result = await _handler.Handle(invalidCommand, default);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(EmailErrors.InvalidFormat, result.Error);
    }

    [Fact]
    public async Task Handle_Should_ReturnError_WhenEmailIsNotUnique()
    {
        // Arrange
        _userRepositoryMock.EmailNotUniqueAsync(Arg.Is<Email>(e => e.Value == Command.Email))
            .Returns(true);

        _userFactoryMock.CreateAsync(Arg.Any<RegisterUserCommand>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Failure<User>(UserErrors.EmailNotUnique));

        // Act
        Result<Guid> result = await _handler.Handle(Command, default);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(UserErrors.EmailNotUnique, result.Error);
    }

    [Fact]
    public async Task Handle_Should_CallRepository_WhenCreateSucceeds()
    {
        // Arrange
        _userRepositoryMock.EmailNotUniqueAsync(Arg.Any<Email>()).Returns(false);

        var user = User.Create(
            Username.Create(Command.Username).Value,
            Email.Create(Command.Email).Value, 
            Command.Password, "link-url");

        _userFactoryMock.CreateAsync(Arg.Any<RegisterUserCommand>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));

        // Act
        Result<Guid> result = await _handler.Handle(Command, default);

        // Assert
        _userRepositoryMock.Received(1).Insert(Arg.Is<User>(u => u.Id == result.Value));
    }

    [Fact]
    public async Task Handle_Should_CallUnitOfWork_WhenCreateSucceeds()
    {
        // Arrange
        _userRepositoryMock.EmailNotUniqueAsync(Arg.Is<Email>(e => e.Value == Command.Email))
            .Returns(false);

        var user = User.Create(
            Username.Create(Command.Username).Value,
            Email.Create(Command.Email).Value,
            Command.Password, "link-url");

        _userFactoryMock.CreateAsync(Arg.Any<RegisterUserCommand>(), Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));

        // Act
        await _handler.Handle(Command, default);

        // Assert
        await _unitOfWorkMock.Received(1).SaveChangesAsync(Arg.Any<CancellationToken>());
    }
}
