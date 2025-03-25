using Modules.Users.Domain.Errors;
using SharedKernel;

namespace Modules.Users.Domain.ValueObjects;

public sealed record Username
{
    private Username(string value) => Value = value;

    public string Value { get; }

    public static Result<Username> Create(string? username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            return Result.Failure<Username>(UsernameErrors.Empty);
        }

        return new Username(username);
    }
}
