using FluentValidation;

namespace StockMarketSimulator.Api.Modules.Users.Application.RevokeRefreshTokens;

internal sealed class RevokeRefreshTokensCommandValidator : AbstractValidator<RevokeRefreshTokensCommand>
{
    public RevokeRefreshTokensCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithMessage("User identifier is required");
    }
}
