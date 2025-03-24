using FluentValidation;

namespace StockMarketSimulator.Api.Infrastructure.Email;

internal sealed class EmailOptionsValidator : AbstractValidator<EmailOptions>
{
    public EmailOptionsValidator()
    {
        RuleFor(x => x.SenderEmail).NotEmpty().EmailAddress();

        RuleFor(x => x.Sender).NotEmpty();

        RuleFor(x => x.Host).NotEmpty();

        RuleFor(x => x.Port)
            .InclusiveBetween(1, 65535)
            .WithMessage("Port must be between 1 and 65535");
    }
}
