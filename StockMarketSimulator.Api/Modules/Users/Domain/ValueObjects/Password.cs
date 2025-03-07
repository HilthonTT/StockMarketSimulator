using SharedKernel;

namespace StockMarketSimulator.Api.Modules.Users.Domain.ValueObjects;

internal sealed record Password
{
    public const int MinLength = 6;

    private Password(string value)
    {
        Value = value;
    }

    public string Value { get; }


    public static Result<Password> Create(string? rawPassword)
    {
        if (string.IsNullOrWhiteSpace(rawPassword))
        {
            return Result.Failure<Password>(PasswordErrors.Empty);
        }

        if (rawPassword.Length < MinLength)
        {
            return Result.Failure<Password>(PasswordErrors.InvalidLength);
        }

        if (!HasRequiredCharacters(rawPassword))
        {
            return Result.Failure<Password>(PasswordErrors.WeakPassword);
        }

        return new Password(rawPassword);
    }

    private static bool HasRequiredCharacters(string password) =>
        password.Any(char.IsUpper) &&
        password.Any(char.IsLower) &&
        password.Any(char.IsDigit) &&
        password.Any(ch => !char.IsLetterOrDigit(ch));
}
