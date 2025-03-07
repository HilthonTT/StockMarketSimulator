using SharedKernel;

namespace StockMarketSimulator.Api.Modules.Users.Domain.ValueObjects;

internal static class EmailErrors
{
    public static readonly Error InvalidFormat = Error.Problem(
        "Email.InvalidFormat",
        "The email format is invalid");

    public static readonly Error Empty = Error.Problem(
        "Email.Empty",
        "The email cannot be empty");
}
