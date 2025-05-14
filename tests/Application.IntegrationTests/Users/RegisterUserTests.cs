using Application.Abstractions.Messaging;
using Application.IntegrationTests.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Modules.Users.Application.Authentication.Register;
using Modules.Users.Domain.Entities;
using SharedKernel;

namespace Application.IntegrationTests.Users;

public sealed class RegisterUserTests : BaseIntegrationTest
{
    private readonly ICommandHandler<RegisterUserCommand, Guid> _handler;

    public RegisterUserTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
        var services = factory.Services;

        _handler = factory.Services.GetRequiredService<ICommandHandler<RegisterUserCommand, Guid>>();
    }

    [Fact]
    public async Task Handle_Should_RegisterUser_WhenCommandIsValid()
    {
        // Arrange
        var command = new RegisterUserCommand(Faker.Internet.Email(), Faker.Internet.UserName(), Faker.Internet.Password());

        // Act
        Result<Guid> result = await _handler.Handle(command, default);

        // Assert
        Assert.True(result.IsSuccess);
    }

    [Fact]
    public async Task Handle_Should_AddUserToDatabase_WhenCommandIsValid()
    {
        // Arrange
        var command = new RegisterUserCommand(Faker.Internet.Email(), Faker.Internet.UserName(), Faker.Internet.Password());

        // Act
        Result<Guid> result = await _handler.Handle(command, default);

        // Assert
        User? user = await UsersDbContext.Users.FindAsync(result.Value);

        Assert.NotNull(user);
    }
}
