using Application.Core.Extensions;
using FluentValidation;
using Modules.Users.Application.Core.Errors;

namespace Modules.Users.Application.Users.LoginWithRefreshToken;

internal sealed class LoginUserWithRefreshTokenCommandValidator : AbstractValidator<LoginUserWithRefreshTokenCommand>
{
    public LoginUserWithRefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithError(UsersValidationErrors.LoginUserWithRefreshToken.RefreshTokenIsRequired);
    }
}
