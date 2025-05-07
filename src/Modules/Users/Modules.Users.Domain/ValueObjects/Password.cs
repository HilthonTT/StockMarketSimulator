using Modules.Users.Domain.Errors;
using SharedKernel;

namespace Modules.Users.Domain.ValueObjects;

public sealed record Password : ValueObject
{
    public const int MinimumLength = 6;

    private Password() { } // Required by EF Core

    private Password(string value) => Value = value;

    public static implicit operator string(Password? password) => password?.Value ?? string.Empty;

    public string Value { get; }

    public static Result<Password> Create(string? password) =>
        Result.Create(password, EmailErrors.Empty)
            .Ensure(p => !string.IsNullOrWhiteSpace(p), PasswordErrors.Empty)
            .Ensure(p => p.Length >= MinimumLength, PasswordErrors.TooShort)
            .Ensure(p => p.Any(char.IsLower), PasswordErrors.MissingLowercaseLetter)
            .Ensure(p => p.Any(char.IsUpper), PasswordErrors.MissingUppercaseLetter)
            .Ensure(p => p.Any(char.IsDigit), PasswordErrors.MissingDigit)
            .Ensure(p => p.Any(c => !char.IsLetterOrDigit(c)), PasswordErrors.MissingNonAlphaNumeric)
        .Map(p => new Password(p));

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
