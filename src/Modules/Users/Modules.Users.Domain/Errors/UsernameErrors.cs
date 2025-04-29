using Modules.Users.Domain.ValueObjects;
using SharedKernel;

namespace Modules.Users.Domain.Errors;

public static class UsernameErrors
{
    public static readonly Error Empty = Error.Problem(
        "Username.Empty", 
        "Username is empty");

    public static readonly Error TooLong = Error.Problem(
        "Username.TooLong",
        $"Username cannot be longer than {Username.MaxLength} characters");
}
