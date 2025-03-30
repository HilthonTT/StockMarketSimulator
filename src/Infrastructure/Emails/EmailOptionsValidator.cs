using FluentValidation;

namespace Infrastructure.Emails;

internal sealed class EmailOptionsValidator : AbstractValidator<EmailOptions>
{
    public EmailOptionsValidator()
    {
        RuleFor(x => x.SenderEmail).NotEmpty().EmailAddress();

        RuleFor(x => x.SenderDisplayName).NotEmpty();

        RuleFor(x => x.SmtpPassword).NotEmpty();

        RuleFor(x => x.SmtpServer).NotEmpty();

        RuleFor(x => x.SmtpPort)
            .InclusiveBetween(1, 65535)
            .WithMessage("Port must be between 1 and 65535");
    }
}
