using Domain.UnitTests.TestData;
using Modules.Budgeting.Domain.ValueObjects;

namespace Domain.UnitTests.Budgeting;

public sealed class MoneyTest
{
    [Fact]
    public void Constructor_ShouldThrowArgumentNullException_WhenCurrencyIsNull()
    {
        // Arrange
        Currency? currency = null;

        // Act
        Action action = () => new Money(default, currency);

        // Assert
        var exception = Assert.Throws<ArgumentNullException>(() => new Money(default, currency));

        Assert.Equal("currency", exception.ParamName);
    }

    [Fact]
    public void Format_ShouldProperlyFormatMoney()
    {
        // Arrange
        const decimal amount = 15.997m;

        Currency currency = CurrencyTestData.DefaultCurrency;

        var money = new Money(amount, currency);

        // Act
        string formatted = money.Format();

        // Assert
        Assert.Equal(currency.Format(amount), formatted);
    }
}
