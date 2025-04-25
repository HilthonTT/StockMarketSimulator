using FluentValidation;

namespace Modules.Users.Application.Users.ChangePassword;

internal sealed class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
{
    public ChangePasswordCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();

        RuleFor(x => x.CurrentPassword).NotEmpty();

        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8);
    }
}
