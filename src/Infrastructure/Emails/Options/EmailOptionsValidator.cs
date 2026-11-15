using FluentValidation;

namespace Infrastructure.Emails.Options;

/// <summary>
/// SMTP settings are optional: with no sender address or password, <see cref="EmailService"/> logs emails
/// instead of sending them (local development). Once credentials are supplied the rest must be valid.
/// </summary>
internal sealed class EmailOptionsValidator : AbstractValidator<EmailOptions>
{
    public EmailOptionsValidator()
    {
        When(x => x.IsSmtpConfigured, () =>
        {
            RuleFor(x => x.SenderEmail).EmailAddress();

            RuleFor(x => x.SmtpServer).NotEmpty();

            RuleFor(x => x.SmtpPort)
                .InclusiveBetween(1, 65535)
                .WithMessage("Port must be between 1 and 65535");
        });
    }
}
