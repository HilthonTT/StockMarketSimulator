using FluentEmail.Core;

namespace StockMarketSimulator.Api.Infrastructure.Email;

internal sealed class EmailService : IEmailService
{
    private readonly IFluentEmail _fluentEmail;

    public EmailService(IFluentEmail fluentEmail)
    {
        _fluentEmail = fluentEmail;
    }

    public Task SendEmailAsync(string email, string subject, string body, CancellationToken cancellationToken = default)
    {
        return _fluentEmail
            .To(email)
            .Subject(subject)
            .Body(body)
            .SendAsync(cancellationToken);
    }
}
