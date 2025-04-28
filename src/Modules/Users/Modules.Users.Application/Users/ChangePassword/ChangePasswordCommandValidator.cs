using Application.Core.Extensions;
using FluentValidation;
using Modules.Users.Application.Core.Errors;

namespace Modules.Users.Application.Users.ChangePassword;

internal sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId).
            NotEmpty().WithError(ValidationErrors.ChangePassword.UserIdIsRequired);

        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithError(ValidationErrors.ChangePassword.CurrentPasswordIsRequired);

        RuleFor(x => x.NewPassword)
            .NotEmpty().WithError(ValidationErrors.ChangePassword.NewPasswordRequired)
            .MinimumLength(8).WithError(ValidationErrors.ChangePassword.NewPasswordIsTooShort);
    }
}
