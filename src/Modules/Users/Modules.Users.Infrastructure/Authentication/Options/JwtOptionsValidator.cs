using FluentValidation;

namespace Modules.Users.Infrastructure.Authentication.Options;

internal sealed class JwtOptionsValidator : AbstractValidator<JwtOptions>
{
    public JwtOptionsValidator()
    {
        RuleFor(x => x.Secret).NotEmpty();

        RuleFor(x => x.ExpirationInMinutes).GreaterThanOrEqualTo(0);

        RuleFor(x => x.Issuer).NotEmpty();

        RuleFor(x => x.Audience).NotEmpty();
    }
}
