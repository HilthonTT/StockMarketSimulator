using SharedKernel;
using StockMarketSimulator.Api.Modules.Users.Domain.ValueObjects;

namespace Domain.UnitTests.Users;

public sealed class PasswordTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    public void Password_Should_ReturnFailure_WhenValueIsNullOrEmpty(string? value)
    {
        // Act
        Result<Password> passwordResult = Password.Create(value);

        // Assert
        Assert.True(passwordResult.IsFailure);
        Assert.Equal(PasswordErrors.Empty, passwordResult.Error);
    }

    [Theory]
    [InlineData("short")]
    [InlineData("12345")]
    public void Password_Should_ReturnFailure_WhenValueIsTooShort(string value)
    {
        // Act
        Result<Password> passwordResult = Password.Create(value);

        // Assert
        Assert.True(passwordResult.IsFailure);
        Assert.Equal(PasswordErrors.InvalidLength, passwordResult.Error);
    }

    [Theory]
    [InlineData("abcdef")] // No uppercase, digit, or special character
    [InlineData("ABCDEF")] // No lowercase, digit, or special character
    [InlineData("abcDEF")] // No digit or special character
    [InlineData("abc123")] // No uppercase or special character
    [InlineData("ABC123")] // No lowercase or special character
    [InlineData("abcdef1")] // No uppercase or special character
    [InlineData("ABCDEF1")] // No lowercase or special character
    [InlineData("Abc123")] // No special character
    public void Password_Should_ReturnFailure_WhenPasswordIsWeak(string value)
    {
        // Act
        Result<Password> passwordResult = Password.Create(value);

        // Assert
        Assert.True(passwordResult.IsFailure);
        Assert.Equal(PasswordErrors.WeakPassword, passwordResult.Error);
    }

    [Theory]
    [InlineData("Abc123!")]
    [InlineData("StrongPass1@")]
    [InlineData("Test#123")]
    [InlineData("XyZ987!@#")]
    public void Password_Should_ReturnSuccess_WhenPasswordIsValid(string value)
    {
        // Act
        Result<Password> passwordResult = Password.Create(value);

        // Assert
        Assert.True(passwordResult.IsSuccess);
        Assert.Equal(value, passwordResult.Value.Value);
    }
}
