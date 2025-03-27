using FluentValidation;

namespace Modules.Users.Application.Users.RevokeRefreshTokens;

internal sealed class RevokeRefreshTokensCommandValidator : AbstractValidator<RevokeRefreshTokensCommand>
{
    public RevokeRefreshTokensCommandValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}
