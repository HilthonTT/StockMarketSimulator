using Application.Core.Extensions;
using FluentValidation;
using Modules.Users.Application.Core.Errors;

namespace Modules.Users.Application.Authentication.RevokeRefreshTokens;

internal sealed class RevokeRefreshTokensCommandValidator : AbstractValidator<RevokeRefreshTokensCommand>
{
    public RevokeRefreshTokensCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty().WithError(UsersValidationErrors.RevokeRefreshTokens.UserIdIsRequired);
    }
}
