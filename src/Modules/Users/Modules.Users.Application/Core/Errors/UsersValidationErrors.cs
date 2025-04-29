using SharedKernel;

namespace Modules.Users.Application.Core.Errors;

internal static class UsersValidationErrors
{
    public static class ChangePassword
    {
        public static readonly Error UserIdIsRequired = Error.Problem(
            "ChangePassword.UserIdIsRequired",
            "The user identifier is required.");

        public static readonly Error CurrentPasswordIsRequired = Error.Problem(
            "ChangePassword.CurrentPasswordIsRequired",
            "The current password is required.");

        public static readonly Error NewPasswordRequired = Error.Problem(
            "ChangePassword.NewPasswordRequired",
            "The new password is required.");

        public static readonly Error NewPasswordIsTooShort = Error.Problem(
            "ChangePassword.NewPasswordIsTooShort",
            "The new password must be at least 8 characters.");
    }

    public static class GetUserById
    {
        public static readonly Error UserIdIsRequired = Error.Problem(
            "GetUserById.UserIdIsRequired",
            "The user identifier is required.");
    }

    public static class LoginUser
    {
        public static readonly Error EmailIsRequired = Error.Problem(
            "LoginUser.EmailIsRequired",
            "The email is required.");

        public static readonly Error EmailFormatIsInvalid = Error.Problem(
            "LoginUser.EmailFormatIsInvalid",
            "The email format is invalid.");

        public static readonly Error PasswordIsRequired = Error.Problem(
           "LoginUser.PasswordIsRequired",
           "The password is required.");
    }

    public static class LoginUserWithRefreshToken
    {
        public static readonly Error RefreshTokenIsRequired = Error.Problem(
            "LoginUserWithRefreshToken.RefreshTokenIsRequired",
            "The refresh token is required.");
    }

    public static class RegisterUser
    {
        public static readonly Error EmailIsRequired = Error.Problem(
           "RegisterUser.EmailIsRequired",
           "The email is required.");

        public static readonly Error EmailFormatIsInvalid = Error.Problem(
            "RegisterUser.EmailFormatIsInvalid",
            "The email format is invalid.");

        public static readonly Error PasswordIsRequired = Error.Problem(
           "RegisterUser.PasswordIsRequired",
           "The password is required.");

        public static readonly Error UsernameIsRequired = Error.Problem(
           "RegisterUser.UsernameIsRequired",
           "The username is required.");

        public static readonly Error PasswordIsTooShort = Error.Problem(
           "RegisterUser.PasswordIsTooShort",
           "The password must be at least 8 characters.");
    }

    public static class ResendEmailVerification
    {
        public static readonly Error EmailIsRequired = Error.Problem(
            "ResendEmailVerification.EmailIsRequired",
            "The email is required.");

        public static readonly Error EmailFormatIsInvalid = Error.Problem(
            "ResendEmailVerification.EmailFormatIsInvalid",
            "The email format is invalid.");
    }

    public static class RevokeRefreshTokens
    {
        public static readonly Error UserIdIsRequired = Error.Problem(
           "RevokeRefreshTokens.UserIdIsRequired",
           "The user identifier is required.");
    }

    public static class UpdateUser
    {
        public static readonly Error UserIdIsRequired = Error.Problem(
          "UpdateUser.UserIdIsRequired",
          "The user identifier is required.");

        public static readonly Error UsernameIsRequired = Error.Problem(
           "UpdateUser.UsernameIsRequired",
           "The username is required.");
    }

    public static class VerifyEmail
    {
        public static readonly Error TokenIdIsRequired = Error.Problem(
            "VerifyEmail.TokenIdIsRequired",
            "The email verification token identifier is required.");
    }
}
