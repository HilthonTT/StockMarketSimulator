using SharedKernel;

namespace StockMarketSimulator.Api.Modules.Users.Domain.ValueObjects;

internal static class PasswordErrors
{
    public static readonly Error Empty = Error.Problem(
        "Password.Empty",
        "The password cannot be empty");

    public static readonly Error InvalidLength = Error.Problem(
        "Password.InvalidLength",
        $"The password must be at least {Password.MinLength} characters long");

    public static readonly Error WeakPassword = Error.Problem(
        "Password.WeakPassword",
        "The password must contain at least one uppercase letter, one lowercase letter, one digit, and one special character");

    public static readonly Error HashingFailed = Error.Problem(
        "Password.HashingFailed",
        "An error occurred while hashing the password."
    );

    public static readonly Error VerificationFailed = Error.Problem(
        "Password.VerificationFailed",
        "The password verification failed. The provided password is incorrect");
}
