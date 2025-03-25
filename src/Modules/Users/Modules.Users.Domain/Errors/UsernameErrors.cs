using SharedKernel;

namespace Modules.Users.Domain.Errors;

public static class UsernameErrors
{
    public static readonly Error Empty = Error.Problem("Username.Empty", "Username is empty");
}
