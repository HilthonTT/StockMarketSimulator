using FluentValidation;

namespace Modules.Stocks.Application.Shorten.Create;

internal sealed class CreateShortenUrlCommandValidator : AbstractValidator<CreateShortenUrlCommand>
{
    public CreateShortenUrlCommandValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty()
            .Must(BeAValidUri)
            .WithMessage("'{PropertyName}' must be a valid URI.");
    }

    private static bool BeAValidUri(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
