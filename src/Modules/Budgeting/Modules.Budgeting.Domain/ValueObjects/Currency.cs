using System.Globalization;
using SharedKernel;

namespace Modules.Budgeting.Domain.ValueObjects;

public sealed class Currency : Enumeration<Currency>
{
    public static readonly Currency Usd = new(1, "US Dollar", "USD");

    public static readonly Currency Eur = new(2, "Euro", "EUR");

    public static readonly Currency Rsd = new(3, "Serbian Dinar", "RSD");

    public static readonly Currency Gbp = new(4, "British Pound", "GBP");

    public static readonly Currency Inr = new(5, "Indian Rupee", "INR");

    public static readonly Currency Jpy = new(6, "Japanese Yen", "JPY");

    public static readonly Currency Cad = new(7, "Canadian Dollar", "CAD");

    public static readonly Currency Aud = new(8, "Australian Dollar", "AUD");

    public static readonly Currency Chf = new(9, "Swiss Franc", "CHF");

    public static readonly Currency Cny = new(10, "Chinese Yuan", "CNY");

    public static readonly Currency Brl = new(11, "Brazilian Real", "BRL");

    public static readonly Currency Jmd = new(12, "Jamaican Dollar", "JMD");

    public static readonly IReadOnlyCollection<Currency> All =
    [
        Usd, Eur, Rsd, Gbp, Cad, Aud, Chf, Cny, Brl, Jmd, Jpy, Inr
    ];


    private static readonly IFormatProvider NumberFormat = new CultureInfo("en-US");

    /// <summary>
    /// Initializes a new instance of the <see cref="Currency"/> class.
    /// </summary>
    /// <param name="value">The currency value.</param>
    /// <param name="name">The currency name.</param>
    /// <param name="code">The currency code.</param>
    private Currency(int id, string name, string code)
        : base(id, name)
    {
        Code = code;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Currency"/> class.
    /// </summary>
    /// <remarks>
    /// Required for deserialization.
    /// </remarks>
    private Currency()
    {
    }

    public string Code { get; private set; }

    public static Currency FromCode(string code)
    {
        return All.FirstOrDefault(x => x.Code == code) ??
            throw new ApplicationException("The currency code is invalid");
    }

    public string Format(decimal amount) => $"{amount.ToString("N2", NumberFormat)} {Code}";
}
