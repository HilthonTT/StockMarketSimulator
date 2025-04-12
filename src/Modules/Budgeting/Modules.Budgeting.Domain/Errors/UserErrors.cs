using SharedKernel;

namespace Modules.Budgeting.Domain.Errors;

public static class UserErrors
{
    public static readonly Error Unauthorized = Error.Forbidden(
        "Users.Unauthorized",
        "You aren't allowed to perform this action or view this resource");

    public static Error NotFound(Guid id) => Error.NotFound(
        "Users.NotFound",
        $"User with Id = '{id}' was not found");
}
