using System.Text.RegularExpressions;
using Modules.Users.Domain.Errors;
using SharedKernel;

namespace Modules.Users.Domain.ValueObjects;

public sealed record Email : ValueObject
{
    public const int MaxLength = 320;

    private const string EmailRegexPattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

    private static readonly Lazy<Regex> EmailFormatRegex =
        new(static () => new Regex(EmailRegexPattern, RegexOptions.Compiled | RegexOptions.IgnoreCase));

    private Email() { } // Required by EF Core

    private Email(string value) => Value = value;

    public string Value { get; }

    public static Result<Email> Create(string? email) =>
        Result.Create(email, EmailErrors.Empty)
            .Ensure(e => !string.IsNullOrWhiteSpace(e), EmailErrors.Empty)
            .Ensure(e => e.Length <= MaxLength, EmailErrors.TooLong)
            .Ensure(EmailFormatRegex.Value.IsMatch, EmailErrors.InvalidFormat)
            .Map(e => new Email(e));

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
