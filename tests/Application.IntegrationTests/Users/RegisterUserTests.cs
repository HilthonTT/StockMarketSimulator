using Application.IntegrationTests.Abstractions;
using Modules.Users.Application.Authentication.Register;
using Modules.Users.Domain.Entities;
using SharedKernel;

namespace Application.IntegrationTests.Users;

public sealed class RegisterUserTests : BaseIntegrationTest
{
    public RegisterUserTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Handle_Should_RegisterUser_WhenCommandIsValid()
    {
        // Arrange
        var command = new RegisterUserCommand(Faker.Internet.Email(), Faker.Internet.UserName(), Faker.Internet.Password());

        // Act
        Result<Guid> result = await Sender.Send(command);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_Should_AddUserToDatabase_WhenCommandIsValid()
    {
        // Arrange
        var command = new RegisterUserCommand(Faker.Internet.Email(), Faker.Internet.UserName(), Faker.Internet.Password());

        // Act
        Result<Guid> result = await Sender.Send(command);

        // Assert
        User? user = await UsersDbContext.Users.FindAsync(result.Value);

        Assert.NotNull(user);
    }
}
