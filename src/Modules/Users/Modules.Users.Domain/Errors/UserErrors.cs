using SharedKernel;

namespace Modules.Users.Domain.Errors;

public static class UserErrors
{
    public static Error NotFound(Guid userId) => Error.NotFound(
       "Users.NotFound",
       $"The user with the Id = '{userId}' was not found");

    public static readonly Error Unauthorized = Error.Forbidden(
        "Users.Unauthorized",
        "You aren't allowed to perform this action or view this resource");

    public static readonly Error NotFoundByEmail = Error.NotFound(
        "Users.NotFoundByEmail",
        "The user with the specified email was not found");

    public static readonly Error EmailNotUnique = Error.Conflict(
        "Users.EmailNotUnique",
        "The provided email is not unique");
}
