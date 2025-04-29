using Application.Core.Extensions;
using FluentValidation;
using Modules.Users.Application.Core.Errors;
using Modules.Users.Domain.ValueObjects;

namespace Modules.Users.Application.Users.ChangePassword;

internal sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId).
            NotEmpty().WithError(UsersValidationErrors.ChangePassword.UserIdIsRequired);

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithError(UsersValidationErrors.ChangePassword.CurrentPasswordIsRequired);

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithError(UsersValidationErrors.ChangePassword.NewPasswordRequired)
            .MinimumLength(Password.MinimumLength).WithError(UsersValidationErrors.ChangePassword.NewPasswordIsTooShort);
    }
}
