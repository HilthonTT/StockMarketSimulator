using Modules.Users.Domain.DomainEvents;
using Modules.Users.Domain.Entities;
using Modules.Users.Domain.ValueObjects;
using SharedKernel;

namespace Domain.UnitTests.Users;

public sealed class UserTests
{
    [Fact]
    public void Create_Should_CreateUser_WhenValid()
    {
        // Arrange
        Username username = Username.Create("test").Value;
        Email email = Email.Create("test@test.com").Value;
        const string passwordHash = "hash-hush";
        const string verificationLink = "https://localhost:5000/url";

        // Act
        var user = User.Create(username, email, passwordHash, verificationLink);

        // Assert
        Assert.NotNull(user);
    }

    [Fact]
    public void Create_Should_RaiseDomainEvent_WhenValid()
    {
        // Arrange
        Username username = Username.Create("test").Value;
        Email email = Email.Create("test@test.com").Value;
        const string passwordHash = "hash-hush";
        const string verificationLink = "https://localhost:5000/url";

        // Act
        var user = User.Create(username, email, passwordHash, verificationLink);

        // Assert
        Assert.Single(user.DomainEvents);
        IDomainEvent domainEvent = user.DomainEvents.First();
        Assert.IsType<UserCreatedDomainEvent>(domainEvent);
    }
}
