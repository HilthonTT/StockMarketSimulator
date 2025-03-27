using SharedKernel;

namespace Modules.Users.Domain.Errors;

public static class EmailVerificationTokenErrors
{
    public static readonly Error Expired = Error.NotFound(
        "EmailVerificationTokenErrors.Expired",
        "The token has already expired");
}
