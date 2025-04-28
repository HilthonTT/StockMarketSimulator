using Application.Core.Extensions;
using FluentValidation;
using Modules.Users.Application.Core.Errors;

namespace Modules.Users.Application.Users.Register;

internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithError(ValidationErrors.RegisterUser.EmailIsRequired)
            .EmailAddress().WithError(ValidationErrors.RegisterUser.EmailFormatIsInvalid);

        RuleFor(x => x.Password)
            .NotEmpty().WithError(ValidationErrors.RegisterUser.PasswordIsRequired)
            .MinimumLength(8).WithError(ValidationErrors.RegisterUser.PasswordIsTooShort);

        RuleFor(x => x.Username)
            .NotEmpty().WithError(ValidationErrors.RegisterUser.UsernameIsRequired);
    }
}
