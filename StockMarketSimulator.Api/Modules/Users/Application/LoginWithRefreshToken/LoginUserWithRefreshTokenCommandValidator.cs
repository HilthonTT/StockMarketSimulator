using FluentValidation;

namespace StockMarketSimulator.Api.Modules.Users.Application.LoginWithRefreshToken;

internal sealed class LoginUserWithRefreshTokenCommandValidator : AbstractValidator<LoginUserWithRefreshTokenCommand>
{
    public LoginUserWithRefreshTokenCommandValidator()
    {
        RuleFor(x => x.RefreshToken)
            .NotEmpty().WithMessage("Refresh token is required");
    }
}
