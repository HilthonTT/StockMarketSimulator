
namespace StockMarketSimulator.Api.Infrastructure.Email;

public interface IEmailService
{
    Task SendEmailAsync(string email, string subject, string body, CancellationToken cancellationToken = default);
}
