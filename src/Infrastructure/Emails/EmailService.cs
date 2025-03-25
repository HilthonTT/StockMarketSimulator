using Application.Abstractions.Emails;
using Contracts.Emails;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;

namespace Infrastructure.Emails;

internal sealed class EmailService(IOptions<EmailOptions> options) : IEmailService
{
    private readonly EmailOptions _options = options.Value;

    public async Task SendEmailAsync(MailRequest mailRequest, CancellationToken cancellationToken = default)
    {
        var email = new MimeMessage()
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
            Body = new TextPart(TextFormat.Text)
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
