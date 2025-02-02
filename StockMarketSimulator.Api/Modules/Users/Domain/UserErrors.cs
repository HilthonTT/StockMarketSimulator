using SharedKernel;

namespace StockMarketSimulator.Api.Modules.Users.Domain;

public static class UserErrors
{
    public static readonly Error EmailNotUnique = Error.Conflict(
        "Users.EmailNotUnique",
        "The provided email is not unique");
}
