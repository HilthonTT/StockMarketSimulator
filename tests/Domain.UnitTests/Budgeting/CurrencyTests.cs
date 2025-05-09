using System.Globalization;
using Domain.UnitTests.TestData;
using Modules.Budgeting.Domain.ValueObjects;

namespace Domain.UnitTests.Budgeting;

public sealed class CurrencyTests
{
    private static readonly IFormatProvider NumberFormat = new CultureInfo("en-US");

    [Fact]
    public void Format_ShouldProperlyFormatAmount()
    {
        // Arrange
        const decimal amount = 1.097m;
        Currency currency = CurrencyTestData.DefaultCurrency;

        // Act
        string formatted = currency.Format(amount);

        // Assert
        Assert.Equal($"{amount.ToString("N2", NumberFormat)} {currency.Code}", formatted);
    }
}
