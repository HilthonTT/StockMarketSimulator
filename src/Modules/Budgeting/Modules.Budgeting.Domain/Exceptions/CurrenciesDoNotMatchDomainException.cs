using Modules.Budgeting.Domain.Errors;
using Modules.Budgeting.Domain.ValueObjects;
using SharedKernel;

namespace Modules.Budgeting.Domain.Exceptions;

public sealed class CurrenciesDoNotMatchDomainException : DomainException
{
    public CurrenciesDoNotMatchDomainException(Currency currency1, Currency currency2) 
        : base(MoneyErrors.CurrenciesDoNotMatch(currency1, currency2))
    {
    }
}
