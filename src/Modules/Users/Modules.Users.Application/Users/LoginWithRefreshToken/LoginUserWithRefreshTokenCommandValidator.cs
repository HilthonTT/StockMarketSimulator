using FluentValidation;

namespace Modules.Users.Application.Users.LoginWithRefreshToken;

internal sealed class LoginUserWithRefreshTokenCommandValidator : AbstractValidator<LoginUserWithRefreshTokenCommand>
{
    public LoginUserWithRefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
