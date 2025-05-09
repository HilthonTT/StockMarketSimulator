using Application.Core.Extensions;
using FluentValidation;
using Modules.Users.Application.Core.Errors;

namespace Modules.Users.Application.Authentication.ResendEmailVerification;

internal sealed class ResendEmailVerificationCommandValidator : AbstractValidator<ResendEmailVerificationCommand>
{
    public ResendEmailVerificationCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithError(UsersValidationErrors.ResendEmailVerification.EmailIsRequired)
            .EmailAddress().WithError(UsersValidationErrors.ResendEmailVerification.EmailFormatIsInvalid);
    }
}
