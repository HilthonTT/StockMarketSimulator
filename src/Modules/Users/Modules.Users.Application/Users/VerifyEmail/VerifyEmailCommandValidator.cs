using Application.Core.Extensions;
using FluentValidation;
using Modules.Users.Application.Core.Errors;

namespace Modules.Users.Application.Users.VerifyEmail;

internal sealed class VerifyEmailCommandValidator : AbstractValidator<VerifyEmailCommand>
{
    public VerifyEmailCommandValidator()
    {
        RuleFor(x => x.TokenId)
            .NotEmpty().WithError(UsersValidationErrors.VerifyEmail.TokenIdIsRequired);
    }
}
