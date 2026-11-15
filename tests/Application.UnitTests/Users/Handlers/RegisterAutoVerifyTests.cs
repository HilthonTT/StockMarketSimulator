using Microsoft.Extensions.Options;
using Modules.Users.Application.Abstractions.Data;
using Modules.Users.Application.Abstractions.Factories;
using Modules.Users.Application.Abstractions.Options;
using Modules.Users.Application.Authentication.Register;
using Modules.Users.Domain.DomainEvents;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.Repositories;
using Modules.Users.Domain.ValueObjects;
using NSubstitute;
using SharedKernel;

namespace Application.UnitTests.Users.Handlers;

public sealed class RegisterAutoVerifyTests
{
    private static readonly RegisterUserCommand Command = new("new.user@test.com", "newuser", "Str0ng!Password");

    [Fact]
    public async Task Register_Should_LeaveTheUserUnverified_ByDefault()
    {
        User user = await RegisterAsync(autoVerifyEmail: false);

        Assert.False(user.EmailVerified);
        Assert.Empty(user.Roles);
        Assert.DoesNotContain(user.DomainEvents, e => e is UserEmailVerifiedDomainEvent);
    }

    [Fact]
    public async Task Register_Should_VerifyThroughTheDomain_WhenAutoVerifyIsOn()
    {
        User user = await RegisterAsync(autoVerifyEmail: true);

        Assert.True(user.EmailVerified);
        Assert.Contains(user.Roles, r => r.Id == Role.Registered.Id);

        // The email-verified event drives budget provisioning and the welcome email, exactly as for a clicked link.
        Assert.Single(user.DomainEvents.OfType<UserEmailVerifiedDomainEvent>());
    }

    private static async Task<User> RegisterAsync(bool autoVerifyEmail)
    {
        var user = User.Create(
            Username.Create(Command.Username).Value,
            Email.Create(Command.Email).Value,
            "hash",
            "https://localhost/verify");

        var userFactory = Substitute.For<IUserFactory>();
        userFactory.CreateAsync(Command, Arg.Any<Guid>(), Arg.Any<CancellationToken>())
            .Returns(Result.Success(user));

        var handler = new RegisterUserCommandHandler(
            userFactory,
            Substitute.For<IUserRepository>(),
            Substitute.For<IEmailVerificationTokenRepository>(),
            Substitute.For<IUnitOfWork>(),
            Substitute.For<IDateTimeProvider>(),
            Options.Create(new UsersOptions { AutoVerifyEmail = autoVerifyEmail }));

        Result<Guid> result = await handler.Handle(Command, default);

        Assert.True(result.IsSuccess);

        return user;
    }
}
