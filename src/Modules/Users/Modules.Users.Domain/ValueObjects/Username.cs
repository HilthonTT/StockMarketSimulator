using Modules.Users.Domain.Errors;
using SharedKernel;

namespace Modules.Users.Domain.ValueObjects;

public sealed record Username : ValueObject
{
    public const int MaxLength = 255;

    private Username() { } // Required by EF Core

    private Username(string value) => Value = value;

    public string Value { get; }

    public static implicit operator string(Username? username) => username?.Value ?? string.Empty;

    public static Result<Username> Create(string? username) =>
        Result.Create(username, UsernameErrors.Empty)
            .Ensure(u => !string.IsNullOrWhiteSpace(username), UsernameErrors.Empty)
            .Ensure(u => u.Length <= MaxLength, UsernameErrors.TooLong)
        .Map(u => new Username(u));

    public override IEnumerable<object> GetAtomicValues()
    {
        yield return Value;
    }
}
