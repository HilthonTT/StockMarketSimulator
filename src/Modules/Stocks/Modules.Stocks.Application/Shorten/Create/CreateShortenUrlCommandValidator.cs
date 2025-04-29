using Application.Core.Extensions;
using FluentValidation;
using Modules.Stocks.Application.Core.Errors;

namespace Modules.Stocks.Application.Shorten.Create;

internal sealed class CreateShortenUrlCommandValidator : AbstractValidator<CreateShortenUrlCommand>
{
    public CreateShortenUrlCommandValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty().WithError(StocksValidationErrors.CreateShortenUrl.UrlIsRequired)
            .Must(BeAValidUri).WithError(StocksValidationErrors.CreateShortenUrl.UrlMustBeValidUrl);
    }

    private static bool BeAValidUri(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out Uri? uriResult)
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
}
