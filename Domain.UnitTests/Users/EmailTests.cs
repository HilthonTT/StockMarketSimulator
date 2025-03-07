using SharedKernel;
using StockMarketSimulator.Api.Modules.Users.Domain.ValueObjects;

namespace Domain.UnitTests.Users;

public sealed class EmailTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("test@test")]  // Missing TLD
    [InlineData("not an email")]  // No @ symbol
    [InlineData("email@domain")]  // Missing TLD
    [InlineData("user@@domain.com")]  // Double @
    [InlineData("user@domain..com")]  // Double dots
    [InlineData(" user@domain.com ")]  // Leading/trailing spaces
    [InlineData("username@")]  // No domain
    [InlineData("@domain.com")]  // No local part
    public void Email_Should_ReturnFailure_WhenValueIsInvalid(string? value)
    {
        // Act
        Result<Email> emailResult = Email.Create(value);

        // Assert
        Assert.True(emailResult.IsFailure);
    }

    [Theory]
    [InlineData("test@test.com")]
    [InlineData("user@gmail.com")]
    [InlineData("john.doe@microsoft.com")]
    [InlineData("user123@domain.io")]  // Numbers in local part
    [InlineData("first-last@company.net")]  // Hyphenated username
    [InlineData("admin@sub.example.com")]  // Subdomain
    [InlineData("user@mail.co.uk")]  // Country code TLD
    [InlineData("test+alias@gmail.com")]  // Plus aliasing
    [InlineData("user_name@domain.org")]  // Underscore in local part
    public void Email_Should_ReturnSuccess_WhenValueIsValid(string value)
    {
        // Act
        Result<Email> emailResult = Email.Create(value);

        // Assert
        Assert.True(emailResult.IsSuccess);
    }
}
