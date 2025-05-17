using FluentValidation;

namespace Modules.Users.Infrastructure.Authentication.Options;

internal sealed class JwtOptionsValidator : AbstractValidator<JwtOptions>
{
    public JwtOptionsValidator()
    {
        RuleFor(x => x.Secret)
            .NotEmpty()
            .MinimumLength(32)
            .WithMessage("JWT secret must be at least 32 characters long for HS256 security.");

        RuleFor(x => x.ExpirationInMinutes)
            .GreaterThan(0)
            .WithMessage("Token expiration must be greater than zero.");

        RuleFor(x => x.Issuer).NotEmpty();

        RuleFor(x => x.Audience).NotEmpty();
    }
}
