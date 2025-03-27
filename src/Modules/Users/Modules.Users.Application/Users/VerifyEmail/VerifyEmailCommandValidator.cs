using FluentValidation;

namespace Modules.Users.Application.Users.VerifyEmail;

internal sealed class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.TokenId).NotEmpty();
    }
}
