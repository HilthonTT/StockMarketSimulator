using Application.Core.Extensions;
using FluentValidation;
using Modules.Users.Application.Core.Errors;
using Modules.Users.Domain.ValueObjects;

namespace Modules.Users.Application.Users.Register;

internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithError(UsersValidationErrors.RegisterUser.EmailIsRequired)
            .EmailAddress().WithError(UsersValidationErrors.RegisterUser.EmailFormatIsInvalid)
            .MaximumLength(Email.MaxLength).WithError(UsersValidationErrors.RegisterUser.EmailIsTooLong);

        RuleFor(x => x.Password)
            .NotEmpty().WithError(UsersValidationErrors.RegisterUser.PasswordIsRequired)
            .MinimumLength(Password.MinimumLength).WithError(UsersValidationErrors.RegisterUser.PasswordIsTooShort);

        RuleFor(x => x.Username)
            .NotEmpty().WithError(UsersValidationErrors.RegisterUser.UsernameIsRequired);
    }
}
