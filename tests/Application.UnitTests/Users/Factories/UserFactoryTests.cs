using Application.UnitTests.Users.TestData;
using Modules.Users.Application.Abstractions.Authentication;
using Modules.Users.Application.Authentication.Factories;
using Modules.Users.Application.Authentication.Register;
using Modules.Users.Domain.Repositories;
using NSubstitute;
using SharedKernel;

namespace Application.UnitTests.Users.Factories;

public sealed class UserFactoryTests
{
    private readonly IUserRepository _userRepositoryMock;
    private readonly IPasswordHasher _passwordHasherMock;
    private readonly IEmailVerificationLinkFactory _emailVerificationLinkFactoryMock;

    private readonly UserFactory _userFactory;

    public UserFactoryTests()
    {
        _userRepositoryMock = Substitute.For<IUserRepository>();
        _passwordHasherMock = Substitute.For<IPasswordHasher>();
        _emailVerificationLinkFactoryMock = Substitute.For<IEmailVerificationLinkFactory>();

        _userFactory = new(_userRepositoryMock, _passwordHasherMock, _emailVerificationLinkFactoryMock);
    }

    [Theory]
    [ClassData(typeof(RegisterUserCommandInvalidData))]
    public async Task CreateAsync_ShouldReturnFailure_WhenArgumentsAreInvalid(RegisterUserCommand command)
    {
        // Act
        Result result = await _userFactory.CreateAsync(command, Guid.CreateVersion7(), default);

        // Assert
        Assert.True(result.IsFailure);
        Assert.False(result.IsSuccess);
    }
}
