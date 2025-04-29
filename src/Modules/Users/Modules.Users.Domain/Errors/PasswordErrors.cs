using Modules.Users.Domain.ValueObjects;
using SharedKernel;

namespace Modules.Users.Domain.Errors;

public static class PasswordErrors
{
    public static readonly Error Empty = Error.Problem(
        "Password.Empty",
        "The password is empty");

    public static readonly Error TooShort = Error.Problem(
        "Password.TooShort",
        $"The password must be at least {Password.MinimumLength} characters");
}
