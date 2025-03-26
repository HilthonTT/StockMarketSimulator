using FluentValidation;

namespace Modules.Users.Application.Users.Register;

internal sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
{
    public RegisterUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();

        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);

        RuleFor(x => x.Username).NotEmpty();
    }
}
