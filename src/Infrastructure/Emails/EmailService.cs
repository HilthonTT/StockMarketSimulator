using Application.Abstractions.Emails;
using Contracts.Emails;
using Infrastructure.Emails.Options;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace Infrastructure.Emails;

internal sealed class EmailService(IOptions<EmailOptions> options, ILogger<EmailService> logger) : IEmailService
{
    private readonly EmailOptions _options = options.Value;

    public async Task SendEmailAsync(
        MailRequest mailRequest,
        bool isHtml = false,
        CancellationToken cancellationToken = default)
    {
        if (!_options.IsSmtpConfigured)
        {
            // Local development without SMTP credentials: log the email (including any verification link)
            // instead of failing, so sign-up and the jobs that send mail still complete.
            logger.LogInformation(
                "SMTP is not configured (Email:SenderEmail / Email:SmtpPassword); email not sent. To: {To}, Subject: {Subject}, Body: {Body}",
                mailRequest.EmailTo,
                mailRequest.Subject,
                mailRequest.Body);

            return;
        }

        using var email = new MimeMessage()
        {
            From =
            {
                new MailboxAddress(_options.SenderDisplayName, _options.SenderEmail),
            },
            To =
            {
                MailboxAddress.Parse(mailRequest.EmailTo)
            },
            Subject = mailRequest.Subject,
            Body = new TextPart(isHtml ? TextFormat.Html : TextFormat.Text)
            {
                Text = mailRequest.Body
            }
        };

        using var smtpClient = new SmtpClient();

        await smtpClient.ConnectAsync(_options.SmtpServer, _options.SmtpPort, SecureSocketOptions.StartTls, cancellationToken);

        await smtpClient.AuthenticateAsync(_options.SenderEmail, _options.SmtpPassword, cancellationToken);

        await smtpClient.SendAsync(email, cancellationToken);

        await smtpClient.DisconnectAsync(true, cancellationToken);
    }
}
