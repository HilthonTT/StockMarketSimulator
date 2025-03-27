using SharedKernel;

namespace Modules.Users.Domain.Errors;

public static class RefreshTokenErrors
{
    public static readonly Error Expired = Error.Problem(
        "RefreshTokens.Expired", "The refresh token has expired");
}
