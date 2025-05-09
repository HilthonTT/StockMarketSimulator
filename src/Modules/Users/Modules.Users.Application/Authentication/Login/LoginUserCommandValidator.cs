using Application.Core.Extensions;
using FluentValidation;
using Modules.Users.Application.Core.Errors;

namespace Modules.Users.Application.Authentication.Login;

internal sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithError(UsersValidationErrors.LoginUser.EmailIsRequired)
            .EmailAddress().WithError(UsersValidationErrors.LoginUser.EmailFormatIsInvalid);

        RuleFor(x => x.Password).NotEmpty().WithError(UsersValidationErrors.LoginUser.PasswordIsRequired);
    }
}
