using Contracts.Emails;

namespace Application.Abstractions.Emails;

public interface IEmailService
{
    Task SendEmailAsync(MailRequest mailRequest, CancellationToken cancellationToken = default);
}
