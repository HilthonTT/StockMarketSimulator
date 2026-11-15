using FluentValidation;

namespace Modules.Users.Infrastructure.Authentication.Options;

internal sealed class AppOptionsValidator : AbstractValidator<AppOptions>
{
    public AppOptionsValidator()
    {
        RuleFor(x => x.PublicApiUrl)
            .NotEmpty()
            .Must(BeAbsoluteHttpUrl)
            .WithMessage("App:PublicApiUrl must be an absolute http(s) URL, e.g. http://localhost:5000.");
    }

    private static bool BeAbsoluteHttpUrl(string value) =>
        Uri.TryCreate(value, UriKind.Absolute, out Uri? uri) &&
        (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
}
