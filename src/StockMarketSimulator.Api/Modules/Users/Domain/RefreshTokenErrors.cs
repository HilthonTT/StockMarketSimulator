using SharedKernel;

namespace StockMarketSimulator.Api.Modules.Users.Domain;

internal static class RefreshTokenErrors
{
    public static readonly Error Expired = Error.Problem(
        "RefreshTokens.Expired", "The refresh token has expired");
}
