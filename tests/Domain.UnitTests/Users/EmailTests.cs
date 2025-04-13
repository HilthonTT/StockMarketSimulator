using Modules.Users.Domain.Errors;
using Modules.Users.Domain.ValueObjects;
using SharedKernel;

namespace Domain.UnitTests.Users;

public sealed class EmailTests
{
    [Theory]
    [InlineData("test@test.com")]
    public void Email_Should_ReturnSuccess_WhenValueIsValid(string value)
    {
        // Act
        Result<Email> result = Email.Create(value);

        // Assert
        Assert.True(result.IsSuccess);

        Email email = result.Value;
        Assert.Equal(email.Value, value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Email_Should_ReturnError_WhenValueIsEmpty(string? value)
    {
        // Act
        Result<Email> result = Email.Create(value);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(EmailErrors.Empty, result.Error);
    }

    [Theory]
    [InlineData("invalid")]
    [InlineData("noatsymbol.com")]
    [InlineData("multiple@@ats.com")]
    public void Email_Should_ReturnError_WhenValueHasInvalidFormat(string value)
    {
        // Act
        Result<Email> result = Email.Create(value);

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(EmailErrors.InvalidFormat, result.Error);
    }
}
