using Modules.Users.Domain.Errors;
using SharedKernel;

namespace Modules.Users.Domain.ValueObjects;

public sealed record Password : ValueObject
{
    public const int MinimumLength = 6;

    private Password() { } // Required by EF Core

    private Password(string value) => Value = value;

    public string Value { get; }

    public static Result<Password> Create(string? password) =>
        Result.Create(password, EmailErrors.Empty)
            .Ensure(p => !string.IsNullOrWhiteSpace(p), PasswordErrors.Empty)
            .Ensure(p => p.Length >= MinimumLength, PasswordErrors.TooShort)
        .Map(p => new Password(p));

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
