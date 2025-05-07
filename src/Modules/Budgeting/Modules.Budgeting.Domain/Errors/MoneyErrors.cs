using Modules.Budgeting.Domain.ValueObjects;
using SharedKernel;

namespace Modules.Budgeting.Domain.Errors;

public static class MoneyErrors
{
    public static Error CurrenciesDoNotMatch(Currency currency1, Currency currency2) => Error.Problem(
        "Money.CurrenciesDoNotMatch", $"The currency {currency1.Code} does not match {currency2.Code}");
}
