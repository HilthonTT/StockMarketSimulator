using Contracts.Emails;

namespace Application.Abstractions.Emails;

public interface IEmailService
{
    Task SendEmailAsync(MailRequest mailRequest, bool isHtml = false, CancellationToken cancellationToken = default);
}
