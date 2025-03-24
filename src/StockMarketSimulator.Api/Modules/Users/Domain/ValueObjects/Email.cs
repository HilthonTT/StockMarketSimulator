using SharedKernel;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace StockMarketSimulator.Api.Modules.Users.Domain.ValueObjects;

internal sealed partial record Email
{
    private static readonly Regex _emailRegex = EmailRegex();

    private Email(string value)
    {
        Value = value.ToLowerInvariant();
    }

    public string Value { get; }  

    public static Result<Email> Create(string? email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Result.Failure<Email>(EmailErrors.Empty);
        }

        if (!MailAddress.TryCreate(email, out _) || !_emailRegex.IsMatch(email))
        {
            return Result.Failure<Email>(EmailErrors.InvalidFormat);
        }

        return new Email(email);
    }

    public override string ToString() => Value;

    [GeneratedRegex(@"^(?!.*\.\.)[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", RegexOptions.IgnoreCase | RegexOptions.Compiled, "en-BE")]
    private static partial Regex EmailRegex();
}
