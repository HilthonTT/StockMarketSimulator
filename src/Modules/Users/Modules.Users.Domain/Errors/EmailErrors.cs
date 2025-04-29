using Modules.Users.Domain.ValueObjects;
using SharedKernel;

namespace Modules.Users.Domain.Errors;

public static class EmailErrors
{
    public static readonly Error Empty = Error.Problem("Email.Empty", "Email is empty");

    public static readonly Error InvalidFormat = Error.Problem(
        "Email.InvalidFormat", "Email format is invalid");

    public static readonly Error TooLong = Error.Problem(
        "Email.TooLong", $"Email cannot be longer than {Email.MaxLength} characters");
}
