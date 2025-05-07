using Modules.Budgeting.Domain.ValueObjects.Exceptions;
using SharedKernel;

namespace Modules.Budgeting.Domain.ValueObjects;

public sealed record Money : ValueObject
{
    public Money(decimal amount, Currency currency)
    {
        Ensure.NotNull(currency, nameof(currency));

        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; private set; }

    public Currency Currency { get; private set; }

    public static Money Sum(Money[] moneyArray)
    {
        if (moneyArray is null || moneyArray.Length == 0)
        {
            throw new ArgumentException("The array of money amounts is null or empty.", nameof(moneyArray));
        }

        Money sum = moneyArray[0];

        foreach (Money money in moneyArray.Skip(1))
        {
            EnsureCurrenciesMatch(sum.Currency, money.Currency);

            sum += money;
        }

        return sum;
    }

    public decimal PercentFrom(Money money)
    {
        EnsureCurrenciesMatch(Currency, money.Currency);

        return Math.Abs(money.Amount / Amount);
    }

    public static Money operator +(Money first, Money second)
    {
        EnsureCurrenciesMatch(first.Currency, second.Currency);

        return new Money(first.Amount + second.Amount, first.Currency);
    }

    public static Money operator -(Money first, Money second)
    {
        EnsureCurrenciesMatch(first.Currency, second.Currency);

        return new Money(first.Amount - second.Amount, first.Currency);
    }

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Amount;
        yield return Currency;
    }

    private static void EnsureCurrenciesMatch(Currency currency1, Currency currency2)
    {
        if (currency1 != currency2)
        {
            throw new CurrenciesDoNotMatchDomainException(currency1, currency2);
        }
    }
}
