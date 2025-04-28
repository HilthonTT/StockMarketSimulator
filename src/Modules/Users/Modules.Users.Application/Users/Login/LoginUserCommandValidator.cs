using Application.Core.Extensions;
using FluentValidation;
using Modules.Users.Application.Core.Errors;

namespace Modules.Users.Application.Users.Login;

internal sealed class LoginUserCommandValidator : AbstractValidator<LoginUserCommand>
{
    public LoginUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithError(ValidationErrors.LoginUser.EmailIsRequired)
            .EmailAddress().WithError(ValidationErrors.LoginUser.EmailFormatIsInvalid);

        RuleFor(x => x.Password).NotEmpty().WithError(ValidationErrors.LoginUser.PasswordIsRequired);
    }
}
